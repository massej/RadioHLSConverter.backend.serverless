# RadioHLSConverter.backend.serverless

This software will allow you to convert an HTTP Live Streaming (HLS) radio source (m3u8 file extension) to the old HTTP stream format.
- Ex : Convert MPEG-TS to ADTS audio format while keeping the same audio codec (aac) from the HLS radio to allow Winamp to play / read the stream.

1. Install C# NET Core 5.0 https://dotnet.microsoft.com/download/dotnet/5.0

	- On Linux you may need to install ffmpeg manually using yum or apt-get.
	
2. Download the last binary version from https://github.com/massej/RadioHLSConverter.backend.serverless/tree/main/release-binaries/

3. appsettings.json configuration
 - Set the HLS radio (m3u8 file extension) source URL and other configurations / infos.

You should set the 
 - HTTPContentType to "audio/aac"
 - "FFMPEGConverterAudioCodec" to "copy"
 - "FFMPEGForceFormat" to "adts"

Note :
 - You should use "copy" on "FFMPEGConverterAudioCodec" to keep the same audio codec as the streaming source to have the best sound quality.

4. Run project 

 - Windows : RadioHLSConverter.backend.serverless.exe --urls="http://127.0.0.1:5000"
 - Linux : /bin/dotnet RadioHLSConverter.backend.serverless.dll --urls="http://127.0.0.1:5000"

 *** You can also install this software as a service (to run in background)

- Windows : sc create RadioHLSConverter BinPath="[YOUR PATH]\RadioHLSConverter.backend.serverless.exe --urls=\"http:/127.0.0.1:5000\""
- Linux : This application support systemd.

5. Add audio source URL into your music software (i.e. Winamp)
 - Add this URL into your music software http://127.0.0.1:5000/api/Radio/
 - If you have multiple HLS radio station then you must use http://127.0.0.1:5000/api/Radio/[id] where you need to replace [id] by your station position in your appsettings.json section AppSettings -> Radios
 - Ex : http://127.0.0.1:5000/api/Radio/0 will be the radio at position 0 in your appsettings.json section AppSettings -> Radios

6. Enjoy listening music to HLS radio stream (m3u8 file extension)
