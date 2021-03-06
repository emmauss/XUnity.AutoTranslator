﻿# XUnity Auto Translator

## Index
 * [Introduction](#introduction)
 * [Translators](#translators)
 * [Text Frameworks](#text-frameworks)
 * [Plugin Frameworks](#plugin-frameworks)
 * [Configuration](#configuration)
 * [Key Mapping](#key-mapping)
 * [Installation](#installation)
 * [Translating Mods](#translating-mods)
 * [Manual Translations](#manual-translations)
 * [Regarding Redistribution](#regarding-redistribution)
 * [Texture Translation](#texture-translation)
 * [Integrating with Auto Translator](#integrating-with-auto-translator)
 
## Introduction
This is a plugin that is capable of using various online translators to provide on-the-fly translations for various Unity-based games.

It does (obviously) go to the internet, in order to provide the translation, so if you are not comfortable with that, don't use it.

Oh, and it is also capable of doing some basic image loading/dumping. Go to the [Texture Translation](#texture-translation) section to find out more.

If you intend on redistributing this plugin as part of a translation suite for a game, please read [this section](#regarding-redistribution).

## Translators
The supported online translators are:
 * [GoogleTranslate](https://anonym.to/?https://translate.google.com/), based on the online Google translation service. Does not require authentication.
   * No limitations, but unstable.
 * [GoogleTranslateLegitimate](https://anonym.to/?https://cloud.google.com/translate/), based on the Google cloud translation API. Requires an API key.
   * Provides trial period of 1 year with $300 credits. Enough for 15 million characters translations.
 * [BingTranslateLegitimate](https://anonym.to/?https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-info-overview), based on the Azure text translation. Requires an API key.
   * Free up to 2 million characters per month.
 * [BaiduTranslate](https://anonym.to/?https://fanyi.baidu.com/), based on Baidu translation service. Requires AppId and AppSecret.
   * Not sure on quotas on this one.
 * [YandexTranslate](https://anonym.to/?https://tech.yandex.com/translate/), based on the Yandex translation service. Requires an API key.
   * Free up to 1 million characters per day, but max 10 million characters per month.
 * [WatsonTranslate](https://anonym.to/?https://cloud.ibm.com/apidocs/language-translator), based on IBM's Watson. Requires a URL, username and password.
   * Free up to 1 million characters per month.
 * [ExciteTranslate](https://anonym.to/?https://www.excite.co.jp/world/english_japanese/), based on the Excite online translation service. Does not require authentication.
   * No limitations, but unstable. Also very slow.
 * Custom. Alternatively you can also specify any custom HTTP url that can be used as a translation endpoint (GET request). This must use the query parameters "from", "to" and "text" and return only a string with the result (try HTTP without SSL first, as unity-mono often has issues with SSL).
   * Example Endpoint=http://my-custom-translation-service.net/translate
   * Example Request: GET http://my-custom-translation-service.net/translate?from=ja&to=en&text=こんにちは
   * Example Response (only body): Hello
   * If someone is willing to implement it, this could provide support for offline translators such as ATLAS or LEC as well, since it enables the scenario where you simply run a local web server that can serve translation requests.
 
**Do note that if you use any of the online translators that does not require some form of authentication, that this plugin may break at any time.**

### About Authenticated Translators
If you decide to use an authenticated service *do not ever share your key or secret*. If you do so by accident, you should revoke it immediately. Most, if not all services provides an option for this.

Also, make sure you monitor the service's request usage/quotas, especially when it is being used with unknown games. This plugin makes no guarantees about how many translation requests will be sent out, although it will attempt to keep the number to a minimum. It does so by using the [spam prevention mechanisms](#spam-prevention) discussed below.

### Spam Prevention
The plugin employs the following spam prevention mechanisms:
 1. When it sees a new text, it will always wait one second before it queues a translation request, to check if that same text changes. It will not send out any request until the text has not changed for 1 second. (Utage (VN Game Engine) is an exception here, as those texts may come from a resource lookup)
 2. It will never send out more than 8000 requests (max 200 characters each (configurable)) during a single game session.
 3. It will never send out more than 1 request at a time (no concurrency!).
 4. If it detects an increasing number of queued translations (3500), the plugin will shutdown.
 5. If the service returns no result for five consecutive requests, the plugin will shutdown.
 6. If the plugin detects that the game queues translations every frame, the plugin will shutdown after 90 frames.
 7. If the plugin detects text that "scrolls" into place, the plugin will shutdown. This is detected by inspecting all requests that are queued for translation. ((1) will genenerally prevent this from happening)
 8. If the plugin consistently queues translations every second for more than 60 seconds, the plugin will shutdown.
 9. For the supported languages, each translatable line must pass a symbol check that detects if the line includes characters from the source language.
 10. It will never attempt a translation for a text that is already considered a translation for something else.
 11. All queued translations are kept track of. If two different components that require the same translation and both are queued for translation at the same time, only a single request is sent.
 12. It employs an internal dictionary of manual translations (~10000 in total) for commonly used phrases (Japanese-to-English only) to prevent sending out translation requests for these.
 13. Some endpoints support batching of translations so far fewer requests are sent. This does not increase the total number of translations per session (2).
 14. All translation results are cached in memory and stored on disk to prevent making the same translation request twice.

## Text Frameworks
The following text frameworks are supported.
 * [UGUI](https://docs.unity3d.com/Manual/UISystem.html)
 * [NGUI](https://assetstore.unity.com/packages/tools/gui/ngui-next-gen-ui-2413)
 * [IMGUI](https://docs.unity3d.com/Manual/GUIScriptingGuide.html) (disabled by default)
 * [TextMeshPro](http://digitalnativestudios.com/textmeshpro/docs/)
 * [Utage (VN Game Engine)](http://madnesslabo.net/utage/?lang=en)

## Plugin Frameworks
The mod can be installed into the following Plugin Managers:
 * [BepInEx](https://github.com/bbepis/BepInEx) (preferred approach)
 * [IPA](https://github.com/Eusth/IPA)
 * UnityInjector

Installation instructions for all methods can be found below.

Additionally it can be installed without a dependency on a plugin manager through ReiPatcher. However, this approach is not recommended if you use one of the above mentioned Plugin Managers!

## Configuration
The default configuration file, looks as such (2.6.0+):

```ini
[Service]
Endpoint=GoogleTranslate         ;Endpoint to use. Can be ["GoogleTranslate", "GoogleTranslateLegitimate", "BingTranslateLegitimate", "BaiduTranslate", "YandexTranslate", "WatsonTranslate", "ExciteTranslate", "*any custom http endpoint*", ""]. If empty, it simply means: Only use cached translations

[General]
Language=en                      ;The language to translate into
FromLanguage=ja                  ;The original language of the game

[Files]
Directory=Translation                                          ;Directory to search for cached translation files
OutputFile=Translation\_AutoGeneratedTranslations.{lang}.txt   ;File to insert generated translations into

[TextFrameworks]
EnableUGUI=True                  ;Enable or disable UGUI translation
EnableNGUI=True                  ;Enable or disable NGUI translation
EnableTextMeshPro=True           ;Enable or disable TextMeshPro translation
EnableIMGUI=False                ;Enable or disable IMGUI translation
EnableUtage=True                 ;Enable or disable Utage-specific translation
AllowPluginHookOverride=True     ;Allow other text translation plugins to override this plugin's hooks

[Behaviour]
Delay=0                          ;Delay to wait before attempting to translate a text in seconds
MaxCharactersPerTranslation=200  ;Max characters per text to translate
IgnoreWhitespaceInDialogue=True  ;Whether or not to ignore whitespace, including newlines, in dialogue keys
IgnoreWhitespaceInNGUI=True      ;Whether or not to ignore whitespace, including newlines, in NGUI
MinDialogueChars=20              ;The length of the text for it to be considered a dialogue
ForceSplitTextAfterCharacters=0  ;Split text into multiple lines once the translated text exceeds this number of characters
CopyToClipboard=False            ;Whether or not to copy hooked texts to clipboard
MaxClipboardCopyCharacters=450   ;Max number of characters to hook to clipboard at a time
EnableUIResizing=True            ;Whether or not the plugin should provide a "best attempt" at resizing UI components upon translation. Only work for NGUI
EnableBatching=True              ;Indicates whether batching of translations should be enabled for supported endpoints
TrimAllText=True                 ;Indicates whether spaces in front and behind translation candidates should be removed before translation
UseStaticTranslations=True       ;Indicates whether or not to use translations from the included static translation cache
OverrideFont=                    ;Overrides the fonts used for texts when updating text components. NOTE: Only works for UGUI
WhitespaceRemovalStrategy=TrimPerNewline ;Indicates how whitespace/newline removal should be handled before attempting translation. Can be ["TrimPerNewline", "AllOccurrences"]
ResizeUILineSpacingScale=        ;A decimal value that the default line spacing should be scaled by during UI resizing, for example: 0.80. NOTE: Only works for UGUI
ForceUIResizing=True             ;Indicates whether the UI resize behavior should be applied to all UI components regardless of them being translated.
IgnoreTextStartingWith=\u180e;   ;Indicates that the plugin should ignore any strings starting with certain characters. This is a list seperated by ';'.

[Texture]
TextureDirectory=Translation\Texture ;Directory to dump textures to, and root of directories to load images from
EnableTextureTranslation=False   ;Indicates whether the plugin will attempt to replace in-game images with those from the TextureDirectory directory
EnableTextureDumping=False       ;Indicates whether the plugin will dump texture it is capable of replacing to the TextureDirectory. Has significant performance impact
EnableTextureToggling=False      ;Indicates whether or not toggling the translation with the ALT+T hotkey will also affect textures. Not guaranteed to work for all textures. Has significant performance impact
EnableTextureScanOnSceneLoad=True ;Indicates whether or not the plugin should scan for textures on scene load. This enables the plugin to find and (possibly) replace more texture
LoadUnmodifiedTextures=False     ;Indicates whether or not unmodified textures should be loaded. Modifications are determined based on the hash in the file name. Only enable this for debugging purposes as it is likely to cause oddities
TextureHashGenerationStrategy=FromImageName ;Indicates how the mod identifies pictures through hashes. Can be ["FromImageName", "FromImageData", "FromImageNameThenData"]

[Http]
UserAgent=                       ;Override the user agent used by APIs requiring a user agent

[GoogleLegitimate]
GoogleAPIKey=                    ;OPTIONAL, needed if GoogleTranslateLegitimate is configured

[BingLegitimate]
OcpApimSubscriptionKey=          ;OPTIONAL, needed if BingTranslateLegitimate is configured

[Baidu]
BaiduAppId=                      ;OPTIONAL, needed if BaiduTranslate is configured
BaiduAppSecret=                  ;OPTIONAL, needed if BaiduTranslate is configured

[Yandex]
YandexAPIKey=                    ;OPTIONAL, needed if YandexTranslate is configured

[Watson]
WatsonAPIUrl=                    ;OPTIONAL, needed if WatsonTranslate is configured
WatsonAPIUsername=               ;OPTIONAL, needed if WatsonTranslate is configured
WatsonAPIPassword=               ;OPTIONAL, needed if WatsonTranslate is configured

[Debug]
EnablePrintHierarchy=False       ;Used for debugging
EnableConsole=False              ;Enables the console. Do not enable if other plugins (managers) handles this
EnableLog=False                  ;Enables extra logging for debugging purposes

[Migrations]
Enable=True                      ;Used to enable automatic migrations of this configuration file
Tag=2.9.0                        ;Tag representing the last version this plugin was executed under. Do not edit
```

## Key Mapping
The following key inputs are mapped:
 * ALT + T: Alternate between translated and untranslated versions of all texts provided by this plugin.
 * ALT + D: Dump untranslated texts (if no endpoint is configured)
 * ALT + R: Reload translation files. Useful if you change the text and texture files on the fly. Not guaranteed to work for all textures.
 * ALT + U: Manual hooking. The default hooks wont always pick up texts. This will attempt to make lookups manually.
 * ALT + F: If OverrideFont is configured, will toggle between overridden and default font.

## Installation
The plugin can be installed in following ways:

### BepInEx Plugin
REQUIRES: [BepInEx plugin manager](https://github.com/bbepis/BepInEx) (follow its installation instructions first!). 

 1. Download XUnity.AutoTranslator-BepIn-{VERSION}.zip from [releases](../../releases).
 2. Extract directly into the game directory, such that the plugin dlls are placed in BepInEx folder.

The file structure should like like this:
```
{GameDirectory}/BepInEx/XUnity.AutoTranslator.Plugin.Core.dll
{GameDirectory}/BepInEx/XUnity.AutoTranslator.Plugin.Core.BepInEx.dll
{GameDirectory}/BepInEx/ExIni.dll
{GameDirectory}/BepInEx/Translation/AnyTranslationFile.txt (these files will be auto generated by plugin!)
```

### IPA Plugin
REQUIRES: [IPA plugin manager](https://github.com/Eusth/IPA) (follow its installation instructions first!).

 1. Download XUnity.AutoTranslator-IPA-{VERSION}.zip from [releases](../../releases).
 2. Extract directly into the game directory, such that the plugin dlls are placed in Plugins folder.

The file structure should like like this
```
{GameDirectory}/Plugins/XUnity.AutoTranslator.Plugin.Core.dll
{GameDirectory}/Plugins/XUnity.AutoTranslator.Plugin.Core.IPA.dll
{GameDirectory}/Plugins/0Harmony.dll
{GameDirectory}/Plugins/ExIni.dll
{GameDirectory}/Plugins/Translation/AnyTranslationFile.txt (these files will be auto generated by plugin!)
 ```

### UnityInjector Plugin
REQUIRES: UnityInjector (follow its installation instructions first!).

 1. Download XUnity.AutoTranslator-UnityInjector-{VERSION}.zip from [releases](../../releases).
 2. Extract directly into the game directory, such that the plugin dlls are placed in UnityInjector folder. **This may not be game root directory!**

The file structure should like like this
```
{GameDirectory}/UnityInjector/XUnity.AutoTranslator.Plugin.Core.dll
{GameDirectory}/UnityInjector/XUnity.AutoTranslator.Plugin.Core.UnityInjector.dll
{GameDirectory}/UnityInjector/0Harmony.dll
{GameDirectory}/UnityInjector/ExIni.dll
{GameDirectory}/UnityInjector/Translation/AnyTranslationFile.txt (these files will be auto generated by plugin!)
 ```
 
### Standalone Installation (ReiPatcher)
REQUIRES: Nothing, ReiPatcher is provided by this download.

 1. Download XUnity.AutoTranslator-ReiPatcher-{VERSION}.zip from [releases](../../releases).
 2. Extract directly into the game directory, such that "SetupReiPatcherAndAutoTranslator.exe" is placed alongside other exe files.
 3. Execute "SetupReiPatcherAndAutoTranslator.exe". This will setup up ReiPatcher correctly.
 4. Execute the shortcut {GameExeName}.lnk that was created besides existing executables. This will patch and launch the game.
 5. From now on you can launch the game from the {GameExeName}.exe instead.

The file structure should like like this
```
{GameDirectory}/ReiPatcher/Patches/XUnity.AutoTranslator.Patcher.dll
{GameDirectory}/ReiPatcher/ExIni.dll
{GameDirectory}/ReiPatcher/Mono.Cecil.dll
{GameDirectory}/ReiPatcher/Mono.Cecil.Inject.dll
{GameDirectory}/ReiPatcher/Mono.Cecil.Mdb.dll
{GameDirectory}/ReiPatcher/Mono.Cecil.Pdb.dll
{GameDirectory}/ReiPatcher/Mono.Cecil.Rocks.dll
{GameDirectory}/ReiPatcher/ReiPatcher.exe
{GameDirectory}/{GameExeName}_Data/Managed/ReiPatcher.exe
{GameDirectory}/{GameExeName}_Data/Managed/XUnity.AutoTranslator.Plugin.Core.dll
{GameDirectory}/{GameExeName}_Data/Managed/0Harmony.dll
{GameDirectory}/{GameExeName}_Data/Managed/ExIni.dll
{GameDirectory}/AutoTranslator/AnyTranslationFile.txt (these files will be auto generated by plugin!)
 ```

## Translating Mods
Often other mods UI are implemented through IMGUI. As you can see above, this is disabled by default. By changing the "EnableIMGUI" value to "True", it will start translating IMGUI as well, which likely means that other mods UI will be translated.

## Manual Translations
When you use this plugin, you can always go to the file `Translation\_AutoGeneratedTranslations.{lang}.txt` (OutputFile) to edit any auto generated translations and they will show up the next time you run the game.

It is also worth noting that this plugin will read all text files (*.txt) in the `Translation` (Directory), so if you want to provide a manual translation, you can simply cut out texts from the `Translation\_AutoGeneratedTranslations.{lang}.txt` (OutputFile) and place them in new text files in order to replace them with a manual translation.

In this context, the `Translation\_AutoGeneratedTranslations.{lang}.txt` (OutputFile) will always have the lowest priority when reading translations. So if the same translation is present in two places, it will not be the one from the (OutputFile) that is used.

## Regarding Redistribution
Redistributing this plugin for various games is absolutely encouraged. However, if you do so, please keep the following in mind:
 * **IMPORTANT: Distribute the _AutoGeneratedTranslations.{lang}.txt file along with the redistribution with as many translations as possible to ensure the online translator is hit as little as possible.**
 * Ensure you keep the plugin up-to-date, as much as reasonably possible.
 * If you use image loading feature, make sure you read [this section](#texture-translation).

## Texture Translation
From version 2.16.0+ this mod provides basic capabilities to replace images. It is a feature that is disabled by default. There is no automatic translation of these images though.

This feature is primarily meant for games with little to no mod support to enable full translations without needing to modify resource files.

It is controlled by the following configuration:

```ini
[Texture]
TextureDirectory=Translation\Texture
EnableTextureTranslation=False
EnableTextureDumping=False
EnableTextureToggling=False
EnableTextureScanOnSceneLoad=False
LoadUnmodifiedTextures=False
TextureHashGenerationStrategy=FromImageName
```

`TextureDirectory` specifies the directory where textures are dumped to and loaded from. Loading will happen from all subdirectories of the specified directory as well, so you can move dumped images to whatever folder structure you desire.

`EnableTextureTranslation` enables texture translation. This basically means that textures will be loaded from the `TextureDirectory` and it's subsdirectories. These images will replace the in-game images used by the game.

`EnableTextureDumping` enables texture dumping. This means that the mod will dump any images it has not already dumped to the `TextureDirectory`. When dumping textures, it may also be worth enabling `EnableTextureScanOnSceneLoad` to more quickly find all textures that require translating. **NEVER REDISTRIBUTE THIS MOD WITH THIS ENABLED.**

`EnableTextureScanOnSceneLoad` allows the plugin to scan for texture objects on the sceneLoad event. This enables the plugin to find more texture at a tiny performance cost during scene load (which is often during loading screens, etc.). However, because of the way Unity works not all of these are guaranteed to be replacable. If you find an image that is dumped but cannot be translated, please report it. However, please recognize this mod is primarily intended for replacing UI textures, not textures for 3D meshes.

`LoadUnmodifiedTextures` enables whether or not the plugin should load textures that has not been modified. This is only useful for debugging, and likely to cause various visual glitches, especially if `EnableTextureScanOnSceneLoad` is also enabled. **NEVER REDISTRIBUTE THIS MOD WITH THIS ENABLED.**

`EnableTextureToggling` enables whether the ALT+T hotkey will also toggle textures. This is by no means guaranteed to work, especially if `EnableTextureScanOnSceneLoad` is also enabled. **NEVER REDISTRIBUTE THIS MOD WITH THIS ENABLED.**

`TextureHashGenerationStrategy` specifies how images are identified. When images are stored, the game will need some way of associating them with the image that it has to replace.
This is done through a hash-value that is stored in square brackets in each image file name, like this: `file_name [0223B639A2-6E698E9272].png`. This configuration specifies how these hash-values are generated:
 * `FromImageName` means that the hash is generated from the internal resource name that the game uses for the image, which may not exist for all images or even be unique. However, it is generally fairly reliable. If an image has no resource name, it will not be dumped.
 * `FromImageData` means that the hash is generated from the data stored in the image, which is guaranteed to exist for all images. However, generating the hash comes at a performance cost, that will also be incurred by the end-users.
 * `FromImageNameThenData` means that it should use the name, if available, otherwise use the data.

There's an important catch you need to be aware when dealing with these options and that is if ANY of these options exists: `EnableTextureDumping=True`, `EnableTextureToggling=True`, `TextureHashGenerationStrategy=FromImageData|FromImageNameThenData`, then the game will need to read the raw data from all images it finds in game in order to replace the image and this is an expensive operation.

It is therefore recommended to use `TextureHashGenerationStrategy=FromImageName`. Most likely, images without a resource name won't be interesting to translate anyway.

If you redistribute this mod with translated images, it is recommended you delete all images you either have no intention of translating or are not translated at all.

You can also change the file name to whatever you desire, as long as you keep the hash appended to the end of the file name.

If you take anything away from this section, it should be these two points:
 * **NEVER REDISTRIBUTE THIS MOD WITH `EnableTextureDumping=True`, `EnableTextureToggling=True` OR `LoadUnmodifiedTextures=True`**
 * **ONLY REDISTRIBUTE THIS MOD WITH `TextureHashGenerationStrategy=FromImageData|FromImageNameThenData` ENABLED IF ABSOLUTELY REQUIRED BY THE GAME.**

### Technical details about Hash Generation in file names
There are actually two hashes in the generated file name, separated by a dash (-):
 * The first hash is a SHA1 (only first 5 bytes) based on the `TextureHashGenerationStrategy` used. If `FromImageName` is specified, then it is based on the UTF8 (without BOM) representation.
 * The second hash is a SHA1 (only first 5 bytes) based on the data in the image. This is used to determine whether or not the image has been modified, so images that has not been edited are not loaded. Unless `LoadUnmodifiedTextures` is specified.

If `TextureHashGenerationStrategy=FromImageData` is specified, only a single hash will appear in each file name, as that single hash can be used both to identify the image and to determine whether or not it has been edited.

## Integrating with Auto Translator

### Implementing a dedicated translation component
As a mod author implementing a translation plugin, you are able to, if you cannot find a translation to a string, simply delegate it to this mod, and you can do it without taking any references to this plugin.

Here's how it works, and what is required:
 * You must implement a Component (MonoBehaviour for instance) that this plugin is able to locate by simply traversing all objects during startup.
 * On this component you must add an event for the text hooks you want to override from XUnity AutoTranslator. This is done on a per text framework basis. The signature of these events must be: Func<object, string, string>. The arguments are, in order: 
    1. The component that represents the text in the UI. (The one that probably has a property called 'text').
    2. The untranslated text
    3. This is the return value and will be the translated text IF an immediate translation took place. Otherwise it will simply be null.
 * The signature for each framework looks like:
    1. UGUI: public static event Func<object, string, string> OnUnableToTranslateUGUI
    2. TextMeshPro: public static event Func<object, string, string> OnUnableToTranslateTextMeshPro
    3. NGUI: public static event Func<object, string, string> OnUnableToTranslateNGUI
    3. IMGUI: public static event Func<object, string, string> OnUnableToTranslateIMGUI
 * Also, the events can be either instance based or static.

### Implementing a component that the Auto Translator should not interfere with
As a mod author, you might not want the Auto Translator to interfere with your mods UI. If this is the case there's two ways to tell Auto Translator not to perform any translation:
 * If your UI is based on GameObjects, you can simply name your GameObjects containing the text element (for example Text class) to something that contains the string "XUAIGNORE". The Auto Translator will check for this and ignore components that contains the string.
 * If your UI is based on IMGUI, the above approach is not possible, because there are no GameObject. In that case you can do the following instead:

```C#
public class MyPlugin : XPluginBase
{
   private GameObject _xua;
   private bool _lookedForXua;

   public void OnGUI()
   {
      // make sure we only do this lookup once, as it otherwise may be detrimental to performance!
      // also: do not attempt to do this in the Awake method or similar of your plugin, as your plugin may be instantiated before the auto translator!
      if(!_lookedForXua)
      {
         _lookedForXua = true;
         _xua = GameObject.Find( "___XUnityAutoTranslator" );
      }

      // try-finally block is important to make sure you re-enable the plugin
      try
      {
         _xua?.SendMessage("DisableAutoTranslator");

         // do your GUI things here
         GUILayout.Button( "こんにちは！" );
      }
      finally
      {
         _xua?.SendMessage("EnableAutoTranslator");
      }
   }
}
```

This approach requires version 2.15.0 or later!
