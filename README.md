# Unity Vosk

This project demonstrates offline speech recognition in Unity using the [Vosk](https://alphacephei.com/vosk/) engine. It contains example scenes and scripts that capture microphone audio and send it to Vosk for realtime speech-to-text processing.

## Requirements

- **Unity 2020.3.48f1** or a compatible 2020.3 LTS version.
- Included native Vosk binaries for Windows, macOS and Android.
- A Vosk speech model. A small English model (`vosk-model-small-en-us-0.15.zip`) is provided in `Assets/StreamingAssets`.

## Opening the project

1. Clone this repository.
2. Start Unity Hub and choose **Open**, then select this folder.
3. Load the `Demo` scene and press **Play** to begin recognition.

## Using models

Place additional model archives inside `Assets/StreamingAssets`. Set the `Model Path` field on `VoskSpeechToText` to the archive name. The model will be extracted to the application `persistentDataPath` the first time it is used.

## License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE) file for details. Third‑party components remain under their own licenses which are included in `Assets/ThirdParty`.
