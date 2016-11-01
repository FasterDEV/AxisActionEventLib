<H2># AxisActionEventLib 1.0</H2>

Base library that provides connectivity with the new Action and Event web-services API of Axis network cameras or devices.

It uses HTTP communication with SOAP/XML message content.

Based on services wsdl's :

- Action : http://www.axis.com/vapix/ws/action1/ActionService.wsdl
- Events : http://www.axis.com/vapix/ws/event1/EventService.wsdl

Simply download the AxisActionEventLib.dll file in the list above and add a new reference to your visual studio project file and use the object browser to explore the ActionEventLib namespace

<H3>Quick usage :</H3>

- Axis devices action and event web-service address : http://yourip/vapix/services

- Use the ActionEventLib.action.ActionService and ActionEventLib.event.EventService objects to query the web-service

<h3>Sample :</h3>

ActionService actionService = new ActionService();</br>
GetActionTemplatesResponse response = await actionService.GetActionTemplatesAsync("IP", "user", "pass");
</br>
EventService eventService = new EventService();</br>
GetEventInstancesResponse Response = await eventService.GetEventsInstancesAsync("IP", "user", "pass");

<h3>Comments :</h3>

- ActionService and EventService inherit abstract base class SOAPRequest. It uses one method sendRequestAsync(...) to send a http request and returns a serviceResponse object which is a base object wrapping the http request response state and XML content. All the other more specific Responses objects inherit from serviceResponse as a base class

- Exceptions are caught and if happen the serviceResponse .isSuccess property will be false and the Content property will contain the exception message

- The device service address is default hardcoded too the address mentionned above, you can change it by assigning the Service_URL property of the service object

<h3>ServiceResponse members:</h3>
<table>
<tr>
<td>bool IsSuccess</td><td>True if the HTTP request was successfull</td>
</tr>
<tr>
<td>HttpStatusCode StatusCode</td><td>HTTP Response status code</td>
</tr>
<tr>
<td>XElement SOAPContent</td><td>A Linq XElement object containing the XML body of HTTP response, if any</td>
</tr>
<tr>
<td>string Content</td><td>The http response content, it can also contain exception messages if one is raised</td>
</tr>
</table>
