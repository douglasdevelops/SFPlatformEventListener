using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Text.Json;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Transport;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Bayeux;
using MessageListnerLib;
using SFEventListenerWeb.Services;
using SFEventListenerWeb;
using static SFEventListenerWeb.EventMessage;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

public class EventController : Controller
{
    private readonly IEventService _eventService;
    private static List<EventMessage> _receivedMessages = new List<EventMessage>();

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [OAuthAuthorize]
    public async Task<ActionResult> Subscribe()
    {
        string accessToken = HttpContext.Request.Cookies["OAuthAccessToken"];
        string instanceUrl = HttpContext.Request.Cookies["InstanceUrl"];

        var result = await _eventService.GetPlatformEventsAsync(instanceUrl, accessToken);
        return View(result);
    }

    [HttpPost]
    public async Task<ActionResult> Subscribe(string eventName)
    {

        string accessToken = HttpContext.Request.Cookies["OAuthAccessToken"];
        string instanceUrl = HttpContext.Request.Cookies["InstanceUrl"];

        EventReceivedHandler handler = new EventReceivedHandler(OnEventReceived);
        _eventService.Subscribe(eventName, accessToken, instanceUrl, handler);

        ViewBag.Message = $"Subscribed to event: {eventName}";
        return View();
    }

    private void OnEventReceived(EventMessage message)
    {
        _receivedMessages.Add(message);
    }

    public IActionResult Messages()
    {
        var messages = _receivedMessages.ToList();
        return PartialView("_Messages", messages);
    }

    [HttpPost]
    public IActionResult Submit(IEnumerable<string> selectedEvents)
    {
        string accessToken = HttpContext.Request.Cookies["OAuthAccessToken"];
        string instanceUrl = HttpContext.Request.Cookies["InstanceUrl"];

        var cookieExpirationTime = DateTime.UtcNow.AddSeconds(7200);

        string serializedList = JsonConvert.SerializeObject(selectedEvents);

        Response.Cookies.Append("SubscribedEvents", serializedList, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = cookieExpirationTime,
            Path = "/"
        });

        if (selectedEvents != null)
        {
            var queryString = string.Join(",", selectedEvents);

            foreach (var platformEvent in selectedEvents)
            {

                EventReceivedHandler handler = new EventReceivedHandler(OnEventReceived);
                _eventService.Subscribe(platformEvent, accessToken, instanceUrl, handler);

                ViewBag.Message += $"Subscribed to event: {platformEvent}";
            }
            return RedirectToAction("Monitor", "Event", new { selectedEvents = queryString });
        }

        return RedirectToAction("Subscribe");
    }

    public ActionResult Monitor(String selectedEvents)
    {
        MonitorViewModel vm = new MonitorViewModel();

        var eventsList = new List<string>();
        if (!string.IsNullOrEmpty(selectedEvents))
        {
            eventsList.AddRange(selectedEvents.Split(','));
        }

        foreach (var event1 in eventsList)
        {
            System.Diagnostics.Debug.WriteLine(event1);
        }

        vm.events = eventsList;

        return View(vm);
    }

}

