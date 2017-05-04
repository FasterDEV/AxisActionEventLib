﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ActionEventLib.action;
using ActionEventLib.types;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ActionEventLib.templates;
using ActionEventLib.events;

namespace ActionEventLibTests
{
    [TestClass]
    public class UT_ActionEventServices
    {
        string VALID_USER = "root";
        string VALID_PASS = "pass";
        string VALID_IP = "192.168.1.223";
        string WRONG_IP = "192.168.1.67777";
        string WRONG_USERPASS = "rooooot";

        #region ActionService

        ActionService actionService = new ActionService();
        ActionRule newActionRule;
        int VALID_ACTIONCONFIG_ID = 18;

        #region Action Templates
        [TestMethod]
        public async Task Get_ActionTemplates()
        {
            GetActionTemplatesResponse response = await actionService.GetActionTemplatesAsync(VALID_IP, VALID_USER, VALID_PASS);

            foreach (KeyValuePair<string,ActionTemplate> t in response.Templates)
            {
                Console.WriteLine("New template : " + t.ToString());
                Console.WriteLine("\t" + "Recipient : " + t.Value.RecipientTemplate);
                Console.WriteLine("\t" + "Ref token : " + (t.Value.recipientTemplateObj != null ? t.Value.recipientTemplateObj.TemplateToken : "Null"));
                Console.WriteLine("Template parameters:");
                foreach (string s in t.Value.Get_Parameters())
                    Console.WriteLine("\t" + s);
            }
            Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && !response.SOAPContent.IsEmpty && response.Templates.Count > 0);
        }
        [TestMethod]
        public async Task Get_ActionTemplatesWithWrongIP_ResponseReturnsFalseAndExplicitExceptionMessage()
        {
            GetActionTemplatesResponse response = await actionService.GetActionTemplatesAsync(WRONG_IP, VALID_USER, VALID_PASS);

            Console.WriteLine("Response IsSuccess : " + response.IsSuccess + " : Message : " + response.Content);

            Assert.IsTrue(!response.IsSuccess && response.Content.Length > 5);
        }
        [TestMethod]
        public async Task Get_ActionTemplatesWithWrongUserPass_ResponseReturnsFalseAndExceptionMessage()
        {
            GetActionTemplatesResponse response = await actionService.GetActionTemplatesAsync(VALID_IP, WRONG_USERPASS, WRONG_USERPASS);

            Console.WriteLine("Response IsSuccess : " + response.IsSuccess + " : Message : " + response.Content);

            Assert.IsTrue(!response.IsSuccess && response.Content.Length > 5);
        }
    #endregion

        #region Action Rules
    [TestMethod]
        public async Task Get_ActionRules()
        {
            GetActionRulesResponse response = await actionService.GetActionRulesAsync(VALID_IP, VALID_USER, VALID_PASS);

            if (!response.IsSuccess)
                Console.WriteLine("NOT 200 OK : " + response.Content);
            else
                foreach (ActionRule ar in response.ActionRules)
                {
                    Console.WriteLine("Action Rule : " + "\r\n" + "\t" + ar.ToString());
                    Console.WriteLine("\t" + "Used action Configuration : " + "\r\n" + "\t" + ar.Configuration.ToString());
                }

            Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && !response.SOAPContent.IsEmpty);
        }

        [TestMethod]
        public async Task Add_ActionRuleWithValidParams_IsSuccessAndReturnsRuleID()
        {
            newActionRule = new ActionRule()
            {
                Name = "UnitTestRule2",
                Enabled = true,
                Trigger = new EventTrigger("tns1:PTZController/tnsaxis:PTZPresets/Channel_1", false, "boolean(//SimpleItem[@Name=\"PresetToken\" and @Value=\"-1\"]) and boolean(//SimpleItem[@Name=\"on_preset\" and @Value=\"1\"])"),
                Configuration = new ActionConfiguration() { ConfigID = VALID_ACTIONCONFIG_ID }
            };

            ServiceResponse response = await actionService.AddActionRuleAsync(VALID_IP, VALID_USER, VALID_PASS , newActionRule);

            if (!response.IsSuccess)
                Console.WriteLine("Error : " + response.Content);
            else
                Console.WriteLine("New action rule id : " + newActionRule.RuleID);

            Assert.IsTrue(response.IsSuccess && !response.SOAPContent.IsEmpty && newActionRule.RuleID != 0);
        }

        /// <summary>
        /// Purpose of this test is to try to create a new action rule with no startevent and a non propertystate eventtrigger instance as condition
        /// This can be faked 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Add_ActionRuleWithInvalidParams()
        {
            newActionRule = new ActionRule()
            {
                Name = "MyBadRule",
                Enabled = true,
                Configuration = new ActionConfiguration() { ConfigID = VALID_ACTIONCONFIG_ID },
                Trigger = new EventTrigger("tns1:VideoSource/tnsaxis:LiveStreamAccessed", true, "boolean(//SimpleItem[@Name=\"accessed\" and @Value=\"1\"]")
            }; 


            ServiceResponse response = await actionService.AddActionRuleAsync(VALID_IP, VALID_USER, VALID_PASS, newActionRule);

            Console.WriteLine("[AddActionRuleWithInvalidParams] Rule ID : " + response.Content);

            Assert.IsTrue(response.IsSuccess);
        }
    #endregion

        #region ActionConfig
        [TestMethod]
            public async Task Get_ActionConfigurations()
            {
                GetActionConfigurationsResponse response = await actionService.GetActionConfigurationsAsync(VALID_IP, VALID_USER, VALID_PASS);

                if (response.IsSuccess)
                    foreach (ActionConfiguration ac in response.Configurations)
                        Console.WriteLine(ac.ToString());

                Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && !response.SOAPContent.IsEmpty);
            }
            [TestMethod]
            public async Task Get_ActionConfigurationsWithWrongCredentials_IsUnauthorized()
            {
                GetActionConfigurationsResponse response = await actionService.GetActionConfigurationsAsync(VALID_IP, "root2", VALID_PASS);

                Assert.IsTrue(response.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized);
            }

            ActionConfiguration m_actionConfiguration;
            [TestMethod]
            public async Task Add_Action_Configurations()
            {
                m_actionConfiguration = new ActionConfiguration()
                {
                    Name = "UnitTestConfig",
                    TemplateToken = "com.axis.action.fixed.notification.http" 
                };

                //Action Template params
                m_actionConfiguration.Parameters.Add("parameters", "action=list");
                m_actionConfiguration.Parameters.Add("message", "Hello world!");
                //Recipient Template params
                m_actionConfiguration.Parameters.Add("upload_url", "http://10.21.66.24");
                m_actionConfiguration.Parameters.Add("login", "root");
                m_actionConfiguration.Parameters.Add("password", "pass");
                m_actionConfiguration.Parameters.Add("proxy_host", "");
                m_actionConfiguration.Parameters.Add("proxy_port", "");
                m_actionConfiguration.Parameters.Add("proxy_login", "");
                m_actionConfiguration.Parameters.Add("proxy_password", "");
                m_actionConfiguration.Parameters.Add("qos", "");

                ServiceResponse response = await actionService.AddActionConfigurationAsync(VALID_IP , VALID_USER , VALID_PASS , m_actionConfiguration);

                Console.WriteLine("[AddActionConfigurations] " + m_actionConfiguration.ConfigID);

                Assert.IsTrue(response.IsSuccess && m_actionConfiguration.ConfigID != 0);
            }

            [TestMethod]
            public async Task Remove_ActionConfigurations()
            {
                ServiceResponse response = await actionService.RemoveActionConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS, VALID_ACTIONCONFIG_ID);
                Console.WriteLine(response.Content);
                if (response.SOAPContent != null)
                    Console.WriteLine(response.SOAPContent);
                Assert.IsTrue(response.IsSuccess && Regex.IsMatch((response.SOAPContent != null ? response.SOAPContent.ToString() : ""), "RemoveActionConfigurationResponse"));
            }
        #endregion

        #region Recipients
        [TestMethod]
        public async Task Get_RecipientTemplates()
        {
            GetRecipientTemplatesResponse response = await actionService.GetRecipientTemplatesAsync(VALID_IP, VALID_USER, VALID_PASS);

            foreach(KeyValuePair<string,RecipientTemplate> t in response.Templates)
            {
                Console.WriteLine("Recipient template token : " + t.Key + "\r\n" + "Template parameters : ");
                foreach (KeyValuePair<string,string> param in t.Value.Parameters)
                    Console.WriteLine("\t" + param.Key);
            }

            Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && !response.SOAPContent.IsEmpty && response.Templates.Count > 0);
        }

        [TestMethod]
        public async Task Add_RecipientConfiguration_NetworkShare()
        {
            RecipientConfiguration recConfig = new RecipientConfiguration()
            {
                Name = "NetworkShare",
                TemplateToken = "com.axis.recipient.networkshare",
                Parameters = new Dictionary<string, string>() {
                     { "upload_path", "VIDEO" } ,
                     { "share_id" , "controlledStorage-AzBy" } ,
                     { "qos" , "" }
                 }
            };

            ServiceResponse addRecipientResponse = await actionService.AddRecipientConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS, recConfig);

            Console.WriteLine("new recipientconfig id : " + recConfig.ConfigurationID);

            //If successfull => IsSuccess should be true and the config ID should be set 
            Assert.IsTrue(addRecipientResponse.IsSuccess && recConfig.ConfigurationID != 0);
        }

        RecipientConfiguration m_recipientConfig;
        [TestMethod]
        public async Task Add_RecipientConfiguration_TCP_Message_Host()
        {
            m_recipientConfig = new RecipientConfiguration()
            {
                TemplateToken = "com.axis.recipient.tcp",
                Name = "TestConfig"
            };

            m_recipientConfig.Parameters.Add("host", VALID_IP);
            m_recipientConfig.Parameters.Add("port", "80");
            m_recipientConfig.Parameters.Add("qos", "");

            ServiceResponse response = await actionService.AddRecipientConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS, m_recipientConfig);

            Console.WriteLine("new recipientconfig id : " + m_recipientConfig.ConfigurationID);
            
            //If successfull => IsSuccess should be true and the config ID should be set 
            Assert.IsTrue(response.IsSuccess && m_recipientConfig.ConfigurationID != 0);
        }

        [TestMethod]
        public async Task Get_RecipientConfig()
        {
            GetRecipientConfigurationsResponse response = await actionService.GetRecipientConfigurationsAsync(VALID_IP, VALID_USER, VALID_PASS);

            Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && !response.SOAPContent.IsEmpty && response.Configurations != null);
        }

        [TestMethod]
        public async Task RemoveRecipientConfigWithValidParams_IsSuccessWithRemoveRecipientConfigurationResponse()
        {
            ServiceResponse response = await actionService.RemoveRecipientConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS,1);

            Assert.IsTrue(response.IsSuccess && response.HttpStatusCode == System.Net.HttpStatusCode.OK && Regex.IsMatch(response.SOAPContent.ToString(), "RemoveRecipientConfigurationResponse"));
        }
        #endregion

        #endregion

        #region EventService

        EventService eventService = new EventService();

        [TestMethod]
        public async Task Get_EventInstances() {
            GetEventInstancesResponse response = await eventService.GetEventsInstancesAsync(VALID_IP, VALID_USER, VALID_PASS);

            foreach (EventTrigger e in response.EventInstances)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("IsPropertyState : " + e.isProperty);
                if (e.Parameters.Count > 0)
                {
                    Console.WriteLine("Parameters :");
                    foreach (KeyValuePair<string,EventTriggerParam> kv in e.Parameters)
                    {
                        Console.WriteLine("\t" + kv.Key);
                        if (kv.Value.DefaultValues.Count > 0)
                        {
                            Console.WriteLine("\t" + "Possible values : ");
                            foreach (string s in kv.Value.DefaultValues)
                                Console.WriteLine("\t\t" + s);
                        }
                    }
                }
            }

            Assert.IsTrue(response.IsSuccess && response.EventInstances.Count > 0);
        }
        [TestMethod]
        public async Task Get_Scheduled_Events()
        {
            GetScheduledEventsResponse response = await eventService.GetScheduledEventsAsync(VALID_IP, VALID_USER, VALID_PASS);

            Console.WriteLine(response.HttpStatusCode + " - " + response.Content);

            if (response.IsSuccess)
                foreach (ScheduledEvent e in response.ScheduledEvents)
                    Console.WriteLine(e.ToString());


            Assert.IsTrue(response.IsSuccess && response.ScheduledEvents.Count > 0);
        }
        [TestMethod]
        public async Task Edit_ScheduledEvent()
        {
            ScheduledEvent newEvent = new ScheduledEvent()
            {
                Name = "UT_DailySchedule",
                Schedule = new ICalendar(new ScheduleTime(8), new ScheduleTime(23), new ScheduleDay[] { ScheduleDay.MO, ScheduleDay.TU, ScheduleDay.WE })
            };

            ServiceResponse resp = await eventService.Edit_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS, "com.axis.schedules.after_hours", newEvent);

            Assert.IsTrue(resp.IsSuccess);
        }
        [TestMethod]
        public async Task Add_Recurrence_ScheduledEvent()
        {
            ScheduledEvent se = new ScheduledEvent()
            {
                Name = "UnitTestScheduledEvent",
                Schedule = new ICalendar(10, PulseInterval.MINUTELY)
            };

            ServiceResponse response = await eventService.Add_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS,se);

            Console.WriteLine(response.Content);

            Assert.IsTrue(response.IsSuccess);
        }
        [TestMethod]
        public async Task Add_Daily_ScheduledEvent()
        {
            ScheduledEvent se = new ScheduledEvent()
            {
                Name = "UnitTestDailyScheduled",
                Schedule = new ICalendar(new ScheduleTime(8,0) , new ScheduleTime(23,0) , new ScheduleDay[] { ScheduleDay.MO, ScheduleDay.TU , ScheduleDay.SA } )
            };

            Console.WriteLine(se.ToString());

            ServiceResponse response = await eventService.Add_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS, se);

            Console.WriteLine(response.Content);

            Assert.IsTrue(response.IsSuccess);
        }
        [TestMethod]
        public async Task Add_Weekly_ScheduledEvent()
        {
            ICalendar calendar = new ICalendar(ScheduleDay.MO, new ScheduleTime(8, 0), ScheduleDay.FR, new ScheduleTime(23, 0));

            ScheduledEvent se = new ScheduledEvent()
            {
                Name = "UnitTestWeeklyScheduled",
                Schedule = calendar
            };

            Console.WriteLine(se.ToString());

            ServiceResponse response = await eventService.Add_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS, se);

            Console.WriteLine(response.Content);

            Assert.IsTrue(response.IsSuccess);
        }
        [TestMethod]
        public async Task Add_Monthly_ScheduledEvent()
        {
            ICalendar calendar = new ICalendar(10, new ScheduleTime(8, 0), 20, new ScheduleTime(17, 30),new ScheduleMonth[] { ScheduleMonth.DEC , ScheduleMonth.SEP });

            ScheduledEvent se = new ScheduledEvent()
            {
                Name = "UnitTestMonthlyScheduledEvent",
                Schedule = calendar
            };

           
            ServiceResponse response = await eventService.Add_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS, se);

            Console.WriteLine(response.Content);

            Assert.IsTrue(response.IsSuccess);
        }
        [TestMethod]
        public async Task Add_Yearly_ScheduledEvent()
        {
            ICalendar calendar = new ICalendar(ScheduleMonth.JAN, 15, new ScheduleTime(8), ScheduleMonth.JUL, 30, new ScheduleTime(22));

            ScheduledEvent se = new ScheduledEvent()
            {
                Name = "UnitTestYearlyScheduled",
                Schedule = calendar
            };

            Console.WriteLine(se.ToString());

            ServiceResponse response = await eventService.Add_ScheduledEventAsync(VALID_IP, VALID_USER, VALID_PASS, se);

            Console.WriteLine(response.Content);

            Assert.IsTrue(response.IsSuccess);
        }

        [TestMethod]
        public async Task Change_VirtualInputStateAsync()
        {
            ServiceResponse resp = await eventService.Change_VirtualInputStateAsync(VALID_IP, VALID_USER, VALID_PASS, 4, true);

            Console.WriteLine(resp.Content);

            Assert.IsTrue(resp.IsSuccess);
        }


        #endregion

        #region Full sammple tests

        //Recommended method to setup an ActionRule, 
        //Setup an ActionRule that triggers on VMD3 motion detection and add an extra virtual input trigger condition
        //  So if Motion is detected and the virtual input state is active the event will trigger
        [TestMethod]
        public async Task Setup_ActionRule_Full_Sample()
        {
            //Sync templates and eventinstances, this could be done once and stored globally 
            //as in 95% of cases all devices share the same templates and event instances
            GetActionTemplatesResponse aTemplates = await actionService.GetActionTemplatesAsync(VALID_IP, VALID_USER, VALID_PASS);
            GetEventInstancesResponse eInstances = await eventService.GetEventsInstancesAsync(VALID_IP, VALID_USER, VALID_PASS);

            //Create an ActionConfiguration first,
            //By passing a ActionTemplate instance, the ActionCofig will directly import the necessary action and associated recipient template parameters
            //To get a quick overview (text output) of the possible template parameters, runt the "Get_ActionTemplates" test method 
            ActionConfiguration sendToNetworkShareConfig = new ActionConfiguration(aTemplates.Templates["com.axis.action.fixed.send_images.networkshare"]);
           
                //Set the action configuration parameters, 
                sendToNetworkShareConfig.Parameters["stream_options"] = "resolution=640x480&amp;videocodec=jpeg&amp;camera=1"; 
                sendToNetworkShareConfig.Parameters["pre_duration"] = "3000"; //milliseconds
                sendToNetworkShareConfig.Parameters["post_duration"] = "3000"; //milliseconds
                sendToNetworkShareConfig.Parameters["max_images"] = "6";
                sendToNetworkShareConfig.Parameters["create_folder"] = ""; 
                sendToNetworkShareConfig.Parameters["filename"] = "image%y-%m-%d_%H-%M-%S-%f.jpg";//Modifiers can be used, see camera help (web-ui \ Events)
                sendToNetworkShareConfig.Parameters["max_sequence_number"] = "6";
                sendToNetworkShareConfig.Parameters["upload_path"] = "VIDEO";
                sendToNetworkShareConfig.Parameters["share_id"] = "controlledStorage-AzBy"; //To find the shareid use http://<ip>/axis-cgi/disks/networkshare/list.cgi?schemaversion=1&shareid=all
                sendToNetworkShareConfig.Parameters["qos"] = "";

            ServiceResponse AddConfigResponse = await actionService.AddActionConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS, sendToNetworkShareConfig);

            Console.WriteLine("Add new action config : Success " + AddConfigResponse.IsSuccess + " response ID : " + AddConfigResponse.Content + " and config ID : " + sendToNetworkShareConfig.ConfigID);

            //VMD3 - TopicExpression - tns1:RuleEngine/tnsaxis:VMD3/vmd3_video_1

            ActionRule newMotionDetectionRule = new ActionRule()
            {
                Name = "MD Rule",
                Enabled = true,
                Trigger = eInstances.EventInstances.Find(x => x.TopicExpression == "tns1:RuleEngine/tnsaxis:VMD3/vmd3_video_1"),
                Configuration = sendToNetworkShareConfig,
            };

            newMotionDetectionRule.SetActivationTimeout(10);
            newMotionDetectionRule.Trigger.Parameters["areaid"].Value = "0"; //Is the default value - for quick overview run Test GetEventInstances
            newMotionDetectionRule.Trigger.Parameters["active"].Value = "1"; //=True, it takes a boolean value but does not work with True / False directly

            //Add the rule to the device, either create a new ServiceResponse instance but you can use the AddConfigResponse instance as well
            AddConfigResponse = await actionService.AddActionRuleAsync(VALID_IP, VALID_USER, VALID_PASS, newMotionDetectionRule);

            Console.WriteLine("Add new action rule : Success " + AddConfigResponse.IsSuccess + " response ID : " + AddConfigResponse.Content + " and rule ID : " + newMotionDetectionRule.RuleID);

            Assert.IsTrue(AddConfigResponse.IsSuccess && AddConfigResponse.Content != "0");
        }


        ///SAMPLE - Create ActionRule - Preset Reached - Led Flashes 5 seconds
        [TestMethod]
        public async Task Setup_ActionRule_Sample_2()
        {
            GetActionTemplatesResponse aTemplates = await actionService.GetActionTemplatesAsync(VALID_IP, VALID_USER, VALID_PASS);
            GetEventInstancesResponse eInstances = await eventService.GetEventsInstancesAsync(VALID_IP, VALID_USER, VALID_PASS);

            //Create an ActionConfiguration first,
            //By passing a ActionTemplate instance, the ActionCofig will directly import the necessary action and associated recipient template parameters
            //To get a quick overview (text output) of the possible template parameters, runt the "Get_ActionTemplates" test method 
            ActionConfiguration BlinkLed = new ActionConfiguration(aTemplates.Templates["com.axis.action.fixed.ledcontrol"]);
 
                BlinkLed.Parameters["interval"] = "250";
                BlinkLed.Parameters["led"] = "statusled";
                BlinkLed.Parameters["color"] = "red,none";
                BlinkLed.Parameters["duration"] = "5";

            ServiceResponse AddConfigResponse = await actionService.AddActionConfigurationAsync(VALID_IP, VALID_USER, VALID_PASS, BlinkLed);
            Console.WriteLine("Add new action config : Success " + AddConfigResponse.IsSuccess + " response ID : " + AddConfigResponse.Content + " and config ID : " + BlinkLed.ConfigID);


            ActionRule PresetReachedRule = new ActionRule()
            {
                Name = "PresetReachedRule",
                Enabled = true,
                Trigger = eInstances.EventInstances.Find(x => x.TopicExpression == "tns1:PTZController/tnsaxis:PTZPresets/Channel_1"),
                Configuration = BlinkLed,
            };

            PresetReachedRule.SetActivationTimeout(10);
            PresetReachedRule.Trigger.Parameters["PresetToken"].Value = "-1"; //See Test GetEventInstances to see eventInstances output and lookup parameters
            PresetReachedRule.Trigger.Parameters["on_preset"].Value = "1"; // Bool value is indicate with 1 / 0

            //Add the rule to the device, either create a new ServiceResponse instance but you can use the AddConfigResponse instance as well
            AddConfigResponse = await actionService.AddActionRuleAsync(VALID_IP, VALID_USER, VALID_PASS, PresetReachedRule);

            Console.WriteLine("Add new action rule : Success " + AddConfigResponse.IsSuccess + " response ID : " + AddConfigResponse.Content + " and rule ID : " + PresetReachedRule.RuleID);

            Assert.IsTrue(AddConfigResponse.IsSuccess && AddConfigResponse.Content != "0");

        }

        #endregion
    }
}