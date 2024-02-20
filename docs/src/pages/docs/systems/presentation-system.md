---
description: A turn-key solution for Virtual Conferences
---

# Presentation System

{% callout type="note" %}
UdonToolkit's Presentation system provides a fully-featured presentation solutions for creating VR Talks, Conferences, or just showing some slides to someone.
{% /callout %}

The system was originally built for the [Prefabs TLX](https://tlx.dev) World Developer conference. You can see it in action in one of the [many talks](https://tlx.dev/talks).

## Features

* A solution to display your presentations with different slide durations, animations and videos
* Slide controls and counters
* Time remaining indicator
* Voice override for the speaker w/ support for extra override zones, e.g., for Q\&A sessions or multiple speakers
* Material swap/object toggles for talk star/end
* Full late-joiner support
* Camera system for showing the speaker
* Stream system for capturing the presentation with talk info and speaker camera overlays
* Personal screen pickup for people to take a closer look
* Laser pointer for the speaker
* Callbacks for all major system events for easier extensibility with your own logic

## Requirements

* TextMeshPro (from Package Manager)
* Cinemachine (from Package Manager)
* UdonSharp v0.19.2 or later
* VRChat SDK 2021.01.28.19.07 or later

## Setup

{% video url="https://www.youtube.com/embed/n_UyLL0rPG0" /%}

* Drag & Drop  the Presentation System prefab from `UdonToolkit/Systems/Presentation System`
* Make sure its in a 0,0,0 spot of the world, its important for some of the tracking objects
* Right click the prefab in the hierarchy and select "Unpack Prefab Completely (Udon does not work well with prefabs)
* Expand the Scene Objects in the hierarchy and move the following objects where you need them
  * **Screen**: the presentation screen. Will display the current slide when the presentation is active
  * **Presentation UI**: the speaker's and host's user interface, place it where your speakers will be talking from
  * **Laser Pointer**: a simple laser pointer pickup to help with explaining the slides content, put near the Presentation UI for ease of use
  * **Presentation Audio Source**: this source will play the presentation audio if it has any. It is a 3D source by default, and is best placed above and in front of where your audience will be sitting. You can make it 2D if you want
  * **Streamer Spot**: a special box where the stream capture user will be teleported when pulling up the stream overlay. Best placed below the stage, slightly behind where the speaker is standing. More on that in the Setting Up Capture section
  * **CM Speaker Cam**: a Cinemachine camera object that captures the speaker. Place somewhere in front of where the speaker will be standing. It will automatically track their head
* Build a test version
* Click Take Control on the UI and try starting a presentation!

## UI Controls

* **Take Control**: takes ownership of the presentation system, shows the UI and removes any currently active voice boosts
* **< and >**: switch between the presentations in the presentations list, does not load the presentation unless Select is pressed
* **Select**: loads the selected presentation
* **Start Talk**: starts the presentation and boosts the speakers's voice ([see below for voice settings](presentation-system#adjusting-voice-settings))
* **Stop**: stops the presentation and removes the voice boost
* **Next Slide**: switches to the next slide
* **Prev Slide**: switches to the previous slide
* **Play Video**: unpauses an animated or video slide (one that has `Auto` checked, [see below](presentation-system#adding-your-presentations))
* **Pause**: pauses an animated or video slide
* **Height Adjust + and -**: moves the UI up and down for ease of use

### Personal Screen Controls

* **Desktop**
  * Press `l` to spawn a personal screen in front of you
* **VR**
  * Press both triggers to spawn a personal screen pick up

This screen contains the presentation slides, the view of the speaker and the presentation info. This is helpful for getting a better look at the slides with small text or when generally being far away from the presentation

### Stream Controls

Press `k` to pull up the stream and be teleported to the configured stream spot position. This will override your view with a special overlay that captures the current speaker and the slides.

{% callout type="warning" %}
Before using Stream Overlay - you need to configure the list of users who are allowed to open it up in the `Presentation Stream Controller`
{% /callout %}

## Configuration

### Creating Presentation Videos

{% callout type="note" %}
Before adding presentations, we need to prepare them so they can be dynamically loaded in VRChat.
{% /callout %}

Most of the presentation software allows you to export a presentation as a video file (like an .mp4 video), exceptions are Web-Based solutions, like Google Slides, but you can still export them as PowerPoint presentations and use it to generate a video.

**Presentation videos work best when they:**

* Are exported with a fixed slide time of 3 seconds
* Have maximum of one video per slide
* Only have one animation sequence per slide
  * e.g., if you have a slide where you want to animate things piece by piece - split them into different. Otherwise - they will all be played in a sequence without pauses, which might be undesirable

When you have your presentation files - upload them to YouTube or Vimeo and set them to be accessible via a link. If you are going to use Vimeo - please use their direct links to avoid any issues

### Adding Presentations

* Expand the hierarchy of the Presentation Player object
* Click on the provided example presentation to see its Udon Behaviour
* The core of the presentation object consists of the 5 main elements
  1. **Talk Url**: the url of the video file with the presentation, YouTube or Vimeo direct link works best
  2. **Talk Title**: the title of the presentation that will be used when selecting a talk, and also displayed for the stream capture
  3. **Talk Author**: the name of the speaker, will also be shown on the stream capture
  4. **Talk Length**: this is only displayed to the speaker to help manage time
  5. **Slides**: a list of slide durations that allow the system to switch between slides

So, to add your own presentations - you need to

* Fill out the fields
* Expand the Configuration Wizard dropdown
* Set the Slide Count to the amount of slides in your presentation
* Set the Slide Duration to the time per slide you set when exporting the video
* Click Configure Talk
* Duplicate the object and repeat for each presentation

When you're done creating individual presentation objects - do the following

* Click on the Presentation Player
* Click Populate Talks button
* This should fill the Talks List with all of your talk objects (they must be parented to the Presentation Player)
* You can expand the Talks list by clicking on the dropdown to see all of your talks. You can move them around to change their order in the presentation UI

{% callout type="note" %}
If you do not have any videos or animations - you're done! You should be able to make a test build and try out your presentations in game!

If you **DO** have animations - look below
{% /callout %}

#### If you have videos or animations on your slides

1. Expand the slides dropdown on the individual presentation and check Auto checkbox near all slides that have animations or videos.
2. Edit the Slide Durations for those slides as follows:

* **For videos**
  * PowerPoint will export slides with videos exactly the matching the length of the videos, so just set those slide durations to the lengths of the included videos
* **For animations**
  * This one is a bit more manual. Open the video you exported and see how much time the slide takes with all the animations included. Most time 3 seconds will be just enough. Otherwise - set the Slide Duration of that slide to the appropriate amount

### Customizing the Screen

{% callout type="note" %}
The Presentation System allows you perform a couple of actions when the presentation starts and ends. This is also where you can adjust the default placeholder material for the screen
{% /callout %}

**Presentation Screen Controller** allows you to swap materials on your screen mesh (or any other mesh) when the talk starts and ends

![](</img/docs/image (53).png>)

If you don't want to swap any materials - uncheck the `Swap Materials` option.

You can configure `Presentation Placeholder` material however you like, or you can swap it to a different material entirely: just drag the new material onto your screen and into the `Default Mats` column for the `Screen` mesh in the list - and you're done!

You can also toggle some objects on and off when the presentation Starts or Ends. For example, you can disable your screen object, and drop it into the `OnTalkStart` and `OnTalkEnd` lists to be enabled and disabled respectively. This will make it so the screen with the slides is only visible when the presentation is going. That would look something like this

![](</img/docs/image (16).png>)

{% callout type="warning" %}
If you do not want any objects to toggle - you can uncheck `Toggle Objects`, note that it will also stop the Speaker Camera from being turned on when the presentation starts starts.
{% /callout %}

### Adjusting Voice Settings

{% callout type="note" %}
By default the Presentation System will boost the voice range of the speaker up to 250 units of distance. It will only boost the person who is currently in control of the presentation UI and clicked the `Start` button.
{% /callout %}

![](</img/docs/image (56).png>)

Most of the configuration is meant to be done by simply sliding the Range Boost slider left and right. You can also use the Gain Tweak parameter to directly affect the volume boost without affecting range, but that can make the speaker too loud when up close, so be careful with it.

{% callout type="note" %}
You can restrict the voice boost only to a set of particular players by clicking `Restrict Access` button and adding the player names to the `Allowed Users` list.
{% /callout %}

Another way of boosting someone's voice is by using `Presentation Voice Zones`, which will boost the volume of anyone who enters its trigger collider. One zone is provided as an example inside `Scene Objects`. It is disabled by default and will not affect anything unless enabled. You also need to check the `Use Zones` checkbox on the `Presentation Voice Controller` for zones to work.

A good use case for zones might be a Q\&A spot where people can come up to and ask their questions to the speaker. That way everyone in the room can hear them

## Stream and Video Capture

{% callout type="note" %}
A big part of creating a presentation is providing a good recording / stream for the people who cannot attend the event personally. UdonToolkit's presentaiton system is fully outfitted to handle that with a Stream/Capture overlay system
{% /callout %}

The system works as follows: when a users presses the stream overlay key, provided they are in the whitelist, they will be teleported to a special streamer spot and get their desktop view overtaken by a special stream overlay. They can press the hotkey again to exit that view.

Default hotkey: `k`

### Stream Configuration

* Move the Streamer Spot object to be positioned below and behind the speaker in such a way that the Streamer Spot Spawn is orientated towards the speaker to provide the best audio capture experience
* Put the Presentation Stream Controller somewhere far from your main world area, by default it is put all the way at x: 1000 y: 1000 to not interfere with the main location
* Inside of the `Presentation Stream Controller` configure the following parameters
  * Set the Reset Position to your world spawn point, the user will be teleported there when exiting the stream view
  * Set the Engage Key to whatever you like
  * Populated the Allowed Users list with the usernames of people who should be able to enable stream overlay, or uncheck Restrict Access to allow everyone to do that (not recommended)
* Inside of the Presentation Stream Controller - find the Stream object and customize the stream overlay UI to your liking

{% callout type="note" %}
For the best viewing experience - record the video with the audio capture set to mono, as spatialization sounds very odd when re-watching the recordings
{% /callout %}

## Full Reference

{% callout type="note" %}
The Configuration guide above includes the core information that should let you configure everything and get started with the presentation system. But if you want to extend it or learn about every single exposed parameter, here's a full reference
{% /callout %}

### Presentation Player

_The top-level behaviour that controls the general flow and sends events to everything else_

#### Parameters

* **Talks**: a list of `TLXTalk` objects that contain the individual talks data
* **Main Player**: the `PresentationVideoPlayer` object that will be active for everyone in the room, responsible for loading and displaying the slides
* **Lookahead Player**: the `PresentationVideoPlayer` object that is only active for the current speaker and is taking care of the Next Slide preview
* **Restrict Access**: specifies whether only particular users will be able to control the Presentation Player
* **Allowed Users**: Contains the list of users that can take control of the Presentation Player. Only visible when Restrict Access is checked, does not have any effect otherwise
* **Callbacks**: a set of callbacks that will be called when a particular event is handled by a Presentation Player
  * **OnTakenControl**: will fire only for the user that just took control of the Presentation Player, you can set the `Target` to `All` to fire the event on all the clients
  * **OnLostControl**: will fire for every user in the instance when someone takes control of the Presentation Player (does not fire on the new owner of the Presentation Player)
  * **OnTalkStart**: will fire for every user in the instance when the presentation starts
  * **OnTalkEnd**: will fire for every user in the instance when the presentation ends
  * **OnNextSlide**: will fire for the current owner of the Presentation Player when they click Next Slide
  * **OnPrevSlide**: will fire for the current owner of the Presentation Player when they click Prev Slide

{% callout type="warning" %}
Callbacks allow you to specify a network target: **Local**, **Owner** or **All**. If the event can be seen by everyone (based on the description above) you **DO NOT** want to use **All**, as that will cause the event to oversync and fire an extra time for each single user in the instance.

So in this case you only want to use **All** on events that only fire for the owner of the Presentation Player. Others should only use **Owner** or **Local**
{% /callout %}

#### Events

* **TakeControl**: takes ownership of the Presentation Player, fires OnTakenControl for the initiator and OnLostControl for everyone else
* **StartTalk**: starts the currently selected presentation, fires OnTalkStart for everyone
* **StopTalk**: stops the currently active presentation, fires OnTalkEnd for everyone
* **PrevSlide**: goes back a slide, fires OnPrevSlide for the current owner
* **NextSlide**: goes forward a slide, fires OnNextSlide for the current owner
* **NextTalk**: selects next presentation from the Talks list and loads it up
* **PrevTalk**: selects previous presentation from the Talks list and loads it up
* **PauseTalkVideo**: pauses an autoplaying presentation slide for everyone
* **PlayTalkVideo**: resumes the autoplaying presentation slide for everyone
* **SelectTalk**(int newIndex): selects the presentation with the specified index and loads it up

### Presentation Talk

_The presentation description class, contains the information about a single presentation and the list of slides_

#### Parameters

* **Talk Url**: the url of the presentation video file, you generally want this to point to a YouTube or Vimeo url
* **Talk Title**: the title of the presentation, will be used in the talk selector UI and the stream capture overlay
* **Talk Author**: the name of the speaker, will be used in the stream capture overlay
* **Talk Length**: the length of the presentation in minutes, will only be used for a timer that is shown to the speaker to help them manage the presentation time
* **Slides**: the list of the presentation slides, internally consists of two arrays
  * **Slide Duration**: the length of a specific slide, if you have a video or an animation within a slide - you will most likely need to adjust this number
  * **Auto**: whether the slide should be autoplayed, this should be checked for all the slides that contain videos or animations

{% callout type="note" %}
When using Vimeo for video hosting - it is recommended to use direct file links in Talk Url. You can also use your own hosting for the video files and use the direct links inside of the Talk URL. Be weary that event attendees will need to click **Allow Untrusted URLs** in VRChat settings.
{% /callout %}

#### Events

{% callout type="warning" %}
`Presentation Talk` is not built to handle incoming events from anything but the Presentation Player, these are provided solely for reference
{% /callout %}

* **Prepare(int slideIndex)**: loads up the presentation video in both main and lookahead players and seeks them to the provided slide index
* **Play**: plays the first slide of the presentation
* **SeekToSlide(int slideIndex)**: seeks main and lookahead players to the provided slide inde
* **Stop**: stops both main and lookahead players
* **NextSlide**: goes forward a slide
* **PrevSlide**: goes back a slide
* **PlayContinuous**: resumes autoplaying slide playback
* **PauseContinuous**: pauses autoplaying slide playback

### Presentation UI Controller

_Drives the Presentation UI functionality_

#### Parameters

* **Presentation Player**: the reference to the main Presentation Player behaviour
* **Presentation UI**: the Presentation UI root Canvas game object
* **Owner Objects**: these objects will be enabled for the new owner when they Take Control of the Presentation Player
* **Public Objects**: these objects will shown to everyone but the owner of the Presentation Player
* **Owner Collider**: the collider that covers the whole of the Presentation UI canvas to provide proper UI detection
* **Public Collider**: the collider that covers only the Take Control button to minimize the player's laser showing up randomly when they're watching the talk
* **Talk Selector Title**: the TextMeshPro component which displays the presentation title in the Presentation Selector on top of the Presentaiton UI
* **Slide Index Text**: the TextMeshPro component which displays the current slide index to the speaker
* **Time Left Text**: the TextMeshPro component which displays the remaining presentation time to the speaker

#### Events

* **SelectorPrev**: selects the previous presentation in the selector and displays its title to the current owner
* **SelectorNext**: selects the next presentation in the selector and displays its title to the current owner
* **SelectorConfirm**: confirms the presentation selection and tells Presentation Player to load it
* **HandleTakeControl**: enables the Owner Objects and disables the Public Objects, meant to be used in the OnTakenControl callback of the Presentation Player
* **HandleLoseControl**: disables the Owner Objects and enables the Public Objects, meant to be used in the OnLostControl callback of the Presentation Player
* **LowerUi**: lowers the Presentaiton UI panel by 0.05, only allows to go down by 2 units maximum
* **RaiseUi**: raises the Presentation UI panel by 0.05, only allows to go up by 2 units maximum
* **HandleTalkStart**: sets the slide index in the Presentation UI and starts the presentation timer, meant to be used in the OnTalkStart callback of the Presentation Player
* **HandleTalkEnd**: resets the slide index and the presentation timer in the Presentation UI, meant to be used in the OnTalkEnd callback of the Presentation Player
* **HandleNextSlide**: sets the slide index in the Presentation UI to the next slide, meant to be used in the OnNextSlide callback of the Presentation Player
* **HandlePrevSlide**: sets the slide index in the Presentation UI to the previous slide, meant to be used in the OnPrevSlide callback of the Presentation Player

{% callout type="note" %}
As you can see, events that start with `Handle` usually meant to be used inside of a Presentation Player callback, and that is the general naming scheme I'm trying to establish
{% /callout %}

### Presentation Screen Controller

_Reacts to the Presentation Start/End events to swap screen materials or toggle game objects_

#### Parameters

* **Swap Materials**: specifies whether to swap materials inside Material Swap Targets
* **Material Swap Targets**: defines a list of meshes which will have their materials swapped when the Presentation Starts and Ends
  * **Meshes**: meshes to swap materials on
  * **Default Mats**: materials to apply when the presentation is stopped
  * **Active Mats**: materials to apply when the presentation starts
* **Toggle Objects**: specifies whether to toggle game objects on presentation start / end
* **OnTalkStart**: list of objects to be affected when the presentation starts
* **OnTalkEnd**: list of objects to be affected when the presentation ends

#### Events

* **HandleTalkStart**: performs the selected actions in the Behaviour parameters, meant to be used in the OnTalkStart callback of the Presentation Player
* **HandleTalkEnd**: performs the selected actions in the Behaviour parameters, meant to be used in the OnTalkEnd callback of the Presentation Player

### Presentation Voice Controller

_Applies voice adjustments to the players_

#### Parameters

* **Restrict Access**: specifies whether only particular users will be able to receive a voice boost
* **Allowed Users**: contains the list of users that can receive the voice boost. Only visible when Restrict Access is checked, does not have any effect otherwise
* **Range Boost**: sets the amount of extra range added to the player's voice
* **Gain Tweak**: sets the amount of gain adjustment for the player's voice. Be careful not to overuse this setting, it can make players really loud up close
* **Use Zones**: allows usage of the `Presentation Voice Zone` behaviours. They will boost the volume of anyone who enters their trigger area
* **Adjust On Presentation Start**: adjusts the voice of the speaker when they start the presentation, and removes the voice boost when they stop or lose ownership of the Presentation Player

#### Events

* **HandleTalkStart**: boosts the current speaker's voice if the Adjust On Presentation Start is checked, meant to be used in the OnTalkStart callback of the Presentation Player
* **HandleTalkEnd**: removes the current's speaker's voice boost if the Adjust On Presentation Start is checked, meant to be used in the OnTalkEnd callback of the Presentation Player
* **NormalizeVolume**: same as HandleTalkEnd, but is meant to be used in the OnTakenControl callback of the Presentation Player
* **EnterZone(VRCPlayerApi player)**: boosts the voice of the provided player if Use Zones is checked
* **ExitZone(VRCPlayerApi player)**: removes the voice boost of the provided player if Use Zones is checked

### Presentation voice Zone

_Detects players entering / exiting the zone and calls voice adjustment events on the `PresentationVoiceController`_

{% callout type="warning" %}
This behaviour requires a trigger collider, and is recommended to be put on a `MirrorReflection` layer. It will warn you about those things, so you don't miss it
{% /callout %}

#### Parameters

* Presentation Voice Controller: a reference to the `PresentationVoiceController` in your scene

### Presentation Stream Controller

_Provides functionality for capturing the presentation_

#### Parameters

* **Restrict Access**: specifies whether only particular users will be able to toggle the stream overlay
* **Allowed Users**: contains the list of users that can open the stream overlay. Only visible when Restrict Access is checked, does not have any effect otherwise
* **Engage Key**: defines the hotkey for opening the stream overlay
* **Stream Objects**: these objects will be toggled when users presses the Engage Key
* **Stream Camera**: the camera that captures the stream UI
* **Stream Animator**: the stream UI animator which controls Idle/Active states
* **Stream Spot**: the position to which the capture player will be teleported to provide best audio capture position
* **Reset Position**: the position to which the capture player will be teleported when they click the Engage Key again - closing the stream overlay
* **Talk Title**: the UI text element with the presentation title on the stream overlay
* **Talk Author**: the UI text element with the presentation author on the stream overlay

#### Events

* **HandleTalkStart**: sets the presentation title and author, and sets the stream UI animator params, meant to be used in the OnTalkStart callback of the Presentation Player
* **HandleTalkEnd**: sets the stream UI animator params, meant to be used in the OnTalkEnd callback of the Presentation Player
* **ToggleStreamObjects**: toggles the objects specified in the Stream Objects list and teleports the local player to the Stream Spot

### Presentation Personal Screen

_Controls the spawnable screen attendees can use to get a better view of the presentation slides and the speaker_

#### Parameters

* **Screen Objects**: a list of objects to be enabled or disabled when user executes the personal screen hotkey / toggle
* **Main Screen**: the personal screen object which will be moved to the Desktop / VR Spawn Ref defined below
* **Stream Controller**: the Presentation Stream Controller that ensures that the correct animator state is set (personal screen shares the same UI capture as the stream overlay)
* **Screen Collider**: the collider on the personal screen, it will get disabled if the user isn't in VR, as it is not a pickup unless the player is in VR
* **Desktop Button**: defines the desktop hotkey used to spawn the personal screen
* **Desktop Spawn Ref**: the position at which to spawn the personal screen on desktop. Should be attached to a player-tracked object somewhere in front of the player so they can easily see it
* **VR Spawn Ref**: the position at which to spawn the personal screen in VR. Should be attached to a player-tracker object somewhere in front of the player so they can easily pick it up

### Laser Pointer

_Provides a basic laser pointing functionality that will project a laser dot onto a surface_

#### Parameters

* **Laser Line**: the liner renderer to be used for the laser line
* **Laser Dot**: the object to be placed on the surface of the hit object
* **Active**: whether the laser pointer is enabled (mostly used internally)
* **Hit Layers**: the list of layers to be considered for collision

{% callout type="warning" %}
The following components are purely internal and are not meant to be modified by the user
{% /callout %}

### Presentation Video Player

_Controls the `VRCUnityVideoPlayer`_

#### Parameters

* V Player: the VRCUnityVideoPlayer to control
* Curr Url: the currently loaded video url

#### Events

* **LoadAndSeek(float time)**: loads the video on the Curr Url, retries if fails or gets rate limited, then seeks the video to the specified time

### Presentation Camera Target

_Tracks the current speaker's position to serve as a camera target_

#### Parameters

* Source: specifies which tracking source to use

#### Events

* **TakeOwnership**: takes ownership of the camera target, the owner will then be used as a tracking target
* **HandleTalkStart**: sets the target of the PersentationCameraTarget to the current owner and enables tracking, meant to used in the OnTalkStart callback of the Presentation Player
* **HandleTalkEnd**: stops the tracking, meant to be used in the OnTalkEnd callback of the Presentation Player
