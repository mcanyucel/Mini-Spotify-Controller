# Mini Spotify Controller

I am not a big fan of the original Spotify App that does not have a mini player. I just want a tiny window that displays current track, and play/pause, next, previous track feeatures. So I have written this small applicatin which uses Spotify Web API to control the streaming. Note that this application does not stream; it just works as a mini controller to the actual device that is streaming the track. It gets the active device from the API, so you can control any source (phone, PC...etc.). It only displays the current track name, artist name and album name to be minimal; but you can always open your streaming app for more information.
*UPDATE:* With versions 2.0.0.1+, the app can also play the track internally. 

**The application requires to have a Spotify Premium account to work. It will not work with free accounts, since free accounts does not support Web API.**

When you run the application, it will open a small window that you need to enter the client id for the Spotify API . You can get the client id from https://developer.spotify.com/dashboard/applications. You need to create an application and get the client id. Then you need to enter the client id to the application. After that, it will open a browser window to authenticate and authorize the application. After you authorize the application, it will close the browser window and start working.

![Sample](./assets/mini-spotify-controller.png "All that the app does")

## Features
* Mini controller to start, pause, next, previous tracks.
* Displays current track, artist and album name.
* Like/Unlike the current track, and instantly get a sharing url copied to the clipboard.
* Can display audio features and audio analysis of the current track.
* Allows to skip to any bar, beat, tatum,segment, or section of the track.
* A randomizer! See below for details.
* Has an internal player to play the track within the app (see below for details).
* Requires a spotify device to be active. If there is no active device, it show a message to open a Spotify app in any of your devices.

## Audio Analysis
It displays a low-level audio analysis of the current track. The audio analysis describes the track's structure and musical content, inluding rhtyhm, pitch, and timbre. For details , see the [Spotify Web API documentation](https://developer.spotify.com/documentation/web-api/reference/tracks/get-audio-analysis/).

Some notable features are:

* Loudness: The overall loudness of a track in decibels (dB). Loudness values are averaged across the entire track and are useful for comparing relative loudness of tracks. Loudness is the quality of a sound that is the primary psychological correlate of physical strength (amplitude). Values typically range between -60 and 0 db.
* Tempo: The overall estimated tempo of a track in beats per minute (BPM). In musical terminology, tempo is the speed or pace of a given piece and derives directly from the average beat duration.
* Time Signature: An estimated overall time signature of a track. The time signature (meter) is a notational convention to specify how many beats are in each bar (or measure).
* Key: The key the track is in. Integers map to pitches using standard Pitch Class notation. E.g. 0 = C, 1 = C♯/D♭, 2 = D, and so on.
* Mode: Mode indicates the modality (major or minor) of a track, the type of scale from which its melodic content is derived. Major is represented by 1 and minor is 0.
* Sections: Audio sections are defined by large variations in rhythm or timbre, e.g. chorus, verse, bridge, guitar solo, etc. Each section contains its own descriptions of tempo, key, mode, time_signature, and loudness.
* Segments: Audio segments attempts to subdivide a song into many segments, with each segment containing a roughly consitent sound throughout its duration. This is done by computing a series of features for each segment, and comparing each new segment to the entire song. The result is a series of data points representing the "distance" between the audio content of each segment and the entire song.
* Beats: The time intervals of beats throughout the track. A beat is the basic time unit of a piece of music; for example, each tick of a metronome. Beats are typically multiples of tatums.
* Tatums: Tatums represent the lowest regular pulse train that a listener intuitively infers from the timing of perceived musical events (segments). Tatums are the smallest regular pulse train that can be perceived in music, roughly analogous to the ticking of a clock.
* Bars: Bars are a musical concept that defines a segment of time corresponding to a specific number of beats in which each beat is represented by a particular note value and the boundaries of the bar are indicated by vertical bar lines.
* Echoprint: Echoprint is a music fingerprinting system that identifies a piece of music based on a short audio sample. It is used by The Echo Nest to identify songs.
* Synchstring: Synchstring is a base64 encoded string that describes the song's structure and musical content. It is used by The Echo Nest to identify songs.
* Rhythmstring: Rhythmstring is a base64 encoded string that describes the song's structure and musical content. It is used by The Echo Nest to identify songs.
* Pitches: Pitch content is given by a “chroma” vector, corresponding to the 12 pitch classes C, C#, D to B, with values ranging from 0 to 1 that describe the relative dominance of every pitch in the chromatic scale. For example a C Major chord would likely be represented by large values of C, E and G (i.e. classes 0, 4, and 7). Vectors are normalized to 1 by their strongest dimension, therefore noisy sounds are likely represented by values that are all close to 1, while pure tones are described by one value at 1 (the pitch) and others near 0. As can be seen below, the 12 vector indices are a combination of low-power spectrum values at their respective pitch frequencies.
* Timbre: Timbre is the quality of a musical note or sound that distinguishes different types of musical instruments, or voices. It is a complex notion also referred to as sound color, texture, or tone quality, and is derived from the shape of a segment’s spectro-temporal surface, independently of pitch and loudness. The timbre feature is a vector that includes 12 unbounded values roughly centered around 0. Those values are high level abstractions of the spectral surface, ordered by degree of importance. For completeness however, the first dimension represents the average loudness of the segment; second emphasizes brightness; third is more closely correlated to the flatness of a sound; fourth to sounds with a stronger attack; etc. See an image below representing the 12 basis functions (i.e. template segments). The actual timbre of the segment is best described as a linear combination of these 12 basis functions weighted by the coefficient values: timbre = c1 x b1 + c2 x b2 + ... + c12 x b12, where c1 to c12 represent the 12 coefficients and b1 to b12 the 12 basis functions as displayed below. Timbre vectors are best used in comparison with each other.

### Randomizer
The randomization flow is as follows:
1. User clicks the randomize button.
2. Set the randomization limit k to 10000.
3. Get the user's 50 (API upper limit) saved tracks from the API with a random offset between 0 and k.
4. If the saved tracks return empty, the offset is greater than the number of user's saved tracks. In this case, halve the k and go to step 3. Otherwise, go to step 6. 
5. If k reaches zero and the saved tracks return empty, then there is no saved track. In this case, show a message to the user and stop.
6. If there are more than 5 tracks (API max seed track count) in the saved tracks, pick 5 random tracks from the saved tracks. Otherwise, pick all of the saved tracks.
7. Send a recommendation request to the API for 100 tracks (API upper limit) with the picked tracks as seed tracks.
8. If the recommendation request returns empty, notify the user that there is no recommendation and stop. Otherwise, go to step 9.
9. Send a request to the API to play all the tracks in the recommendation response.
10. Start groovin'!.

### Internal Player:
The internal player is a WebView2 control that uses Spotify Web Playback API. It is invisible and it is used to play the track within the application. It is not displayed in the devices list until it is initialized. It takes some time to initialize, so it may appear few seconds after the application is started.

The internal player uses Spotify Web Playback API. This API needs HTTPS connection to work, so we cannot simply load an HTML string to web view. Instead, the app trails the following sequence of events to play the track internally (The events with the same ordinal number such as 1a and 1b are not synchronous; they may or may not be concurrent):

1a. Once the `MainViewModel` is initialized and the user authorizes the application, `MainViewModel` calls `IResourceService.GetWebPlayerPath(string accessToken)` method (note that `AccessToken` will be available here), which:

* Gets the HTML content template as string from the `assets.Resources.WebPlayer.txt` resource file,
* Replaces the `{{accessToken}}` placeholder with the actual access token and `{{internalPlayerName}}` placeholder,
* Writes this source to a temporary file in the user's temp directory as "webplayer.html",
* Returns the path of the temporary file.

2a. If the returned internal player path string is not empty, `MainViewModel` sets this value to the `InternalPlayerPath` property.

1b. `MainWindow` sets the environment variable `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` as `--autoplay-policy=no-user-gesture-required` so that the internal player can play the track without user interaction.

2b. Once the `MainWindow` content is rendered;

* It defines the environment for webview2 control as *User/LocalApplicationData/MiniSpotifyController*, and wait until the webview2 control is initialized. 
* Once the webview2 control is initialized, `MainWindow` attaches a hook to the `WebMessageReceived` event of the webview2 control. This is necessary to get the player id of the internal player as this id cannot be assigned manually and it is required to transfer playback to the internal player. This hook updates `MainViewModel.InternalPlayerId` property. Note that the *Webplayer.txt* source file contains a script that sends the player id to the host application as a web message (TODO: This id is possibly no longer needed as we treat the local player as any other player).
* It listens to the `MainViewModel.InternalPlayerHTMLPath` property changes (it will start as null); once the internal player HTML source path is set, it calls `InitializeInternalPlayer` method which does the following:
    * It sets up a virtual host for webview2 because spotify web playback API uses EME (Encrypted Media Extensions) which requires HTTPS connection. It is not possible to create a HTTPS connection while loading a string or a local file, so we create a virtual host (with name `VIRTUAL_HOST_NAME` to load the internal player source. The CORS of this virtual host is set to `CoreWebView2HostResourceAccessKind.Deny` for security. Note that the virtual host needs to be a directory, so we get the directory of the internal player source file and set it as the virtual host.
    * It navigates to the virtual host with the internal player source file using HTTPS connection.
    * The player is ready to play once the navigation is completed. 

## TODO
* [ ] Add a feature to display the current track's lyrics.

## New Features

### 2.0.1.3
* Fixed a bug where seeking using the Audio Analysis window breaks the auto update of the track in the audio analysis window.

### 2.0.1.1
* Added audio analysis. See above for details.

### 2.0.0.1
* Added an invisible WebView2 to the `MainVindow` for internal playback (i.e. playback within the app). It uses Spotify Web Playback API. **TODO	**: It takes some time for the internal player to initialize, during which it is not displayed in the devices list. Maybe add a loading indicator for this?

### 1.3.3.0

* Update the .NET version from 7.0 to 8.0.
* Update third party libraries to the latest versions.
* Add a feature to display the available devices. User can select a device to play the track on that device.
* Updated many `ObservableObject` classes to use attribute-based MVVM features. This makes the code cleaner and more readable. `MainViewModel` is an exception because it is a bit complex and I did not want to break it. Also due to its complexity, attribute-based approach may not result in a cleaner code.

### 1.1.5

* Make always on top feature optional. It can be enabled/disabled from the top bar button with a pin icon.

### 1.1.4

* Song radio feature is added. This is basically the randomize function with the currently playing song as the only starting seed.
* Changed all one line functions from body block to expression block.
* Changed settings button from text to icon.
* Fixed a bug where if the randomization/song radio fails, the play button was stuck as hourglass icon.

### 1.1.3

* Added randomizer feature. See below for details.
* Fixed a bug where the image cover art was updated every second (which was not necessary).

### 1.1.2

* Added a "Stats for Nerds" style "audio features" window. It can be accessed by the button on the app bar with a chart icon.

### 1.1.1

* Added a feature to share. Clicking the share button will copy the currently playing track's Spotify url to the clipboard.

### 1.0.0

* Added a feature to display if the current track is added to the library or not. User can click the heart icon to add or remove the track from the library. Added tracks will be displayed with a red heart icon, and removed tracks will be displayed with a light gray heart icon.

## Requirements

* Windows 10.0.17763.0 or newer. This is because the application uses `Microsoft.Toolkit.Uwp.Notifications` library which requires Windows 10.0.17763.0 or newer.
* .NET 8.0
* Spotify Premium account

## 3rd Party Dependencies

* Mahapps.Metro (UI): A great UI library for WPF applications. https://mahapps.com/
* Mahaps.Metro.IconPacks: A great icon library for WPF applications.
* CommunityToolkit.MVVM: A great MVVM library for WPF applications.
* Microsoft.Extensions.DependencyInjection: A great DI library for .NET applications.
* Microsoft.Toolkit.Uwp.Notifications: A great library for Windows toast notifications.
* Microsoft.Web.WebView2: A great library for Chromium based webview. It is used to display the user authentication and authorization page.
* Microsoft.Xaml.Behaviors.Wpf: A great library for WPF behaviors (mainly used for binding commands to event handlers).
