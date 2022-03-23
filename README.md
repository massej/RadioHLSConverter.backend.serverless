# RadioHLSConverter.backend.serverless

This software will allow you to convert an HTTP Live Streaming (HLS) radio source (m3u8 file extension) to the old HTTP streaming format.
- Ex : Convert "MPEG-TS" to "ADTS" audio format while keeping the same audio codec "AAC" from the HLS radio to allow Winamp to play and read the stream.

1. Install C# NET Core 5.0 https://dotnet.microsoft.com/download/dotnet/5.0

	- On Linux you may need to install ffmpeg manually using yum or apt-get.

2. Download the lastest binary version from https://github.com/massej/RadioHLSConverter.backend.serverless/tree/main/release-binaries/

3. appsettings.json configuration Host
 - "ForceHTTPS" Force to use encrypted HTTPS traffic. (HTTP will be redirected to HTTPS and enable HSTS - Strict-Transport-Security HTTP header.)
 - "AllowedHosts" Host or IP that is allowed to connect to HTTP server. ("*" is any hosts.)
 - "FFMpegPipeBufferInBytes" stream pipe buffer in bytes between the application and ffmpeg.
 - "BufferSizeInSeconds" buffer size in seconds between the application and the HLS server.
 - "RadioName" radio name to send to the music application.
 - "RadioDescription" radio description to send to the music application.
 - "RadioSourceURL" HTTP Live Streaming (HLS) radio source URL.
 - "HTTPContentType" HTTPContentType to send to the music application. You must use the same as your output format. i.e. if your HLS streaming source is an "AAC" audio codec then you should use "audio/aac".
	(See HTTPContentType documentation https://developer.mozilla.org/en/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types)
 - "FFMPEGConverterAudioCodec" ffmpeg output audio codec, you should use "copy" to keep the same audio codec as the streaming source to have the best sound quality.
	(See ffmpeg codecs documentation https://ffmpeg.org/ffmpeg-codecs.html)
 - "FFMPEGForceFormat" ffmpeg output audio format. If your HLS streaming source is an "AAC" audio codec under "MPEG-TS" format then you should use "adts" for better quality.
	(See ffmpeg formats documentation https://ffmpeg.org/ffmpeg-formats.html)
 - "FFMPEGCustomArgument" you can specify an additional parameter to send to ffmpeg. (Ex : "-b:a 192k" will force the output to 192 kbps but keep in mind that if your audio source is a 128 kbps then you will not gain any sound improvement.)
	(See ffmpeg documentation https://ffmpeg.org/ffmpeg.html)

4. Run project

 - Windows : RadioHLSConverter.backend.serverless.exe --urls="http://127.0.0.1:5000"
 - Linux : /bin/dotnet RadioHLSConverter.backend.serverless.dll --urls="http://127.0.0.1:5000"

 *** You can also install this software as a service. (i.e. To have the application running in background.)

- Windows : sc create RadioHLSConverter BinPath="[YOUR PATH]\RadioHLSConverter.backend.serverless.exe --urls=\"http://127.0.0.1:5000\""
- Linux : This application support systemd. (See Linux manual "man" documentation https://man7.org/linux/man-pages/man1/systemd.1.html)

5. Add audio source URL into your music software (i.e. Winamp)
 - Add this URL into your music software http://127.0.0.1:5000/api/Radio/
 - If you have multiple HLS radio station then you must use http://127.0.0.1:5000/api/Radio/[id] where you need to replace [id] by your station position in your appsettings.json section AppSettings -> Radios.
 - Ex : http://127.0.0.1:5000/api/Radio/0 will be the radio at position 0 in your appsettings.json section AppSettings -> Radios.

6. Enjoy listening music to your favourite HLS radio station!
