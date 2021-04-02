# TIDAL

A WPF-based interface to a [Transmission](https://github.com/transmission/transmission)
 BitTorrent client.

 ## Notes

### Settings
 All of the settings for the app are stored in your
 ```AppData\Local``` directory in a folder named
 ```Tidal-Net48```. Why the weird naming? Because I've got
 more than one version of the app floating about on my
 system; one for .NET Core, another for .NET 5, and yet
 another frustratingly incomplete one for UWP. Ugh. Every
 time I think I've got a handle on UWP, it slaps me upside
 the head and reminds me of who's really in charge here.
 Screw that noise.

 At this point, I'm not smart enough to release the one for
 .NET Core; I can't get the deployment to work correctly
 since it keeps asking to download Core 3.1, then when I do,
 it ignores that and asks again. Like I said: Not smart
 enough.

 Besides, the .NET 4.8 version works great and usually has a
 smaller memory footprint than the newer Core releases. Why
 is that? I have no idea. 

 But I digress; back to the settings. They're in the
 ```Tidal-Net48``` folder and are all stored in JSON. You
 could edit them manually, but I wouldn't.

### App Structure
The application uses the Prism library to handle the details
of MVVM, navigation, and messaging. Using Prism in an app like
this is a bit of overkill, but then I'm reassured by all the
people who buy SUVs with no intent of ever taking them offroad.

The messaging system of Prism is rather baroque, so I found a
nice interface out there somewhere that makes it a little easier
to comprehend, which is good, since the entire app is nothing more
than an elaborate message-pump.

So yeah, there's the ```ITaskService``` that handles running
a little task -- oddly enough -- periodically. There's a task for
asking for torrents, another for getting the session information,
another for getting the free space. Each of those tasks just sends
a message:

    private async Task RequestAllTorrents()
    {
        if (IsOpen)
        {
            messenger.Send(new TorrentRequest());
            await Task.CompletedTask;
        }
    }

That's it. That's the entirety of getting torrents. Somewhere else
in the view model there's the method subscribed to the message that
returns the torrents themselves.

    private void OnTorrents(TorrentResponse torrentResponse)
    {
        torrentStatusService.CheckStatus(torrentResponse.Torrents);
        torrentStatusService.CheckForConnection(torrentResponse.Torrents);
    }

Of course, this is the ```ShellViewModel``` which doesn't do any
real displaying or anything, so it's simple here, but you get the
gist of it.

The thing is, is that the whole app is like this, where almost
everything is messages sent and messages responded to. I like it. It's
probably not canon, though, I'll admit that; I'm probably abusing the
hell out of a system that wasn't meant to do what the app is doing
with it.

The ```ShellViewModel``` is where almost all of the messages 
originate, sending messages to query the host about every 3 seconds
or so. Other view models, like ```MainViewModel``` respond to
those messages in their own ways, like displaying the information.

All of the requests and corresponding responses are handled in the
```IBrokerService```, which sits around, waiting for a new
request to come in. It puts it in a queue, then a background
task processes it. The ```IBrokerService``` is the **only**
aspect of the app that talks to the actual client, handling just
one request/response pairing at a time, obviating the age-old
problem of handling reentrancy. One point of contact.

