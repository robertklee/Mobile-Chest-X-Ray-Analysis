# Mobile Chest X-Ray Analysis

# Contents

1. [Introduction](#introduction)
1. [Device Compatibility](#device-compatibility)
1. [Features](#features)
1. [Project Structure](#project-structure)
1. [Getting Started with Development](#getting-started-with-development)
1. [Integrating AI](#integrating-ai)
1. [Troubleshooting](#troubleshooting)
1. [Disclaimer](#disclaimer)
1. [License](#license)
1. [Contribute](#contribute)
1. [Acknowledgements](#acknowledgements)

# Introduction

Mobile Chest X-Ray Analysis is a research project to showcase the [ChestXRay model](https://github.com/Azure/AzureChestXRay) in an iOS and Android app, demonstrating how developers can infuse AI into their own mobile applications.

This app uses [Xamarin.iOS](https://docs.microsoft.com/en-us/xamarin/ios/) and [Xamarin.Android](https://docs.microsoft.com/en-us/xamarin/android/) which enables application logic to be written in a shared C# codebase, while the UI can be built using the native platform tools (storyboards for iOS and XML layouts for Android).

The AI model is consumed using [CoreML](https://developer.apple.com/documentation/coreml) on iOS and [TensorFlow-Android](https://github.com/tensorflow/tensorflow/tree/master/tensorflow/contrib/android) on Android.

# Device Compatibility

|          |                             |
| -------- | --------------------------- |
| iOS      | 11.2+                       |
| Android  | API Level 19+ (KitKat 4.4+) |

# Features

* Import a digital x-ray image from:
  * Camera roll / photo library
  * Local filesystem
  * Cloud storage
* Receive an image shared from another app
* Take a photo of a physical x-ray with the camera
* Crop the image (zoom, pan, rotate<sup>1</sup>)
* Run the ChestXRay ML model
* Main results page for the top 3 results (conditions with the highest likelihoods)
  * Interactive view of the x-ray (zoom and pan)
  * Select a condition to see a Class Activation Map (CAM) for that condition overlayed on the original x-ray
* See More page with results for all 14 diagnosable chest conditions
  * 2 Interactive x-ray views:
    * Original image
    * Analyzed image (includes the CAM overlay for the selected condition)

<sup>1</sup> On Android precise rotation is supported. On iOS, only 90&deg; rotations are supported.

# Project Structure

## App

This folder contains the actual application that consists of one Visual Studio Solution containing 5 separate projects:

### XRayAnalysis
* Code that is common to both the iOS and Android apps
  * AI
    * Base classes/interfaces to provide an abstract API for the platform-specific implementations of running the AI models
    * Post-processing the outputs of the AI models for Class Activation Map (CAM) generation and interpeting the likelihood results
  * Image pre-processing/post-processing
    * Resizing the images for input to the AI models
    * Color-mapping the CAM data for overlaying onto the x-ray images
  * Analytics
    * Convenience methods and strings for Visual Studio App Center

### XRayAnalysis.Android
* Core Android application code and UI
  * XML resources: Layouts/colors/styles etc.
  * Icon assets
  * Activity classes
  * TensorFlow-Android based implementation of the AI classes

### XRayAnalysis.iOS
* Core iOS application code and UI
  * Storyboards/XIBs etc.
  * Icon assets
  * ViewControllers
  * CoreML based implementation of the AI classes

### ShareExtension
* Integrates with iOS Share Extension functionality, allowing the user to share an image from another app into the iOS application
  * Storyboard for the View that appears when the extension icon is tapped
  * ViewController
  * App Groups used to share image to containing application

### TensorFlowAndroid
* [Binding library](https://docs.microsoft.com/en-us/xamarin/android/platform/binding-java-library/binding-an-aar) for the TensorFlow-Android AAR (retrieved from maven central)
  * The TensorFlowAndroid.csproj file contains a custom build target (`TensorFlowRestore`) that runs a Bash or PowerShell script to automatically download the TensorFlow-Android AAR from maven central before a build begins (if the file is already downloaded, no action will be taken)

## Design_Assets

This folder contains design assets used in the app.

## Model_Prep

This folder contains python scripts that can be used to convert a trained Keras model to mobile compatible formats.

For details on this process, see the [Integrating AI](#integrating-ai) section.

# Getting Started with Development

## Setup on Windows (as your development machine)

### Windows 7 or higher

### Visual Studio 2017 version 15.5+ and Xamarin 4.5.0+
1. Follow the [Windows Installation instructions](https://docs.microsoft.com/en-us/xamarin/cross-platform/get-started/installation/windows#Installation) to install Visual Studio and the required Xamarin components

### Mac Agent (required to build iOS app)
* Building the iOS app requires access to a device running macOS (if you are only planning on building the Android app, this is not necessary)
* You can still do your development on a Windows machine, as long as the macOS device is on the same LAN
* Follow the instructions for [Connecting to a Mac](https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/windows/connecting-to-mac/) to set this up

## Setup on Mac (as your development machine)

### macOS Sierra 10.12.6+

1. Update your OS if you are on a version older than 10.12.6

### Xcode 9.2+

1. Download and install Xcode from the Mac App Store
1. Launch Xcode
1. Read and accept the License Agreements
1. Let Xcode install the additional components (this includes command-line tools that Visual Studio will need)
1. You can close Xcode at this point or leave it open

### Visual Studio for Mac

* Follow the official Xamarin documentation to [Setup and Install Visual Studio for Mac](https://docs.microsoft.com/en-us/visualstudio/mac/installation)
  * Make sure you select at least the following options when choosing what to install:
    * Android
    * iOS
    * .NET Core
  * For Android, make sure both "Java SDK" and "Android SDK" are selected
    * If you already have the Java SDK and/or Android SDK installed, you can deselect those options here, and later [set the SDK locations in Visual Studio](https://docs.microsoft.com/en-us/xamarin/android/troubleshooting/questions/android-sdk-location?tabs=vsmac)

## Getting the Source Code
1. Clone the repository to your development machine
    ```
    git clone https://github.com/Microsoft/Mobile-Chest-X-Ray-Analysis.git
    ```
1. Open `XRayAnalysis.sln` (in the App folder) in Visual Studio
    * If you receive an error message about the Android platform version you have installed, this will be addressed in the following sections

## Building/Running the Android App

### SDK Setup
1. In Visual Studio launch the Android SDK Manager:
    * On Windows: `Tools > Android > Android SDK Manager`
    * On a Mac: `Tools > SDK Manager`
1. Follow the guide for using the [Visual Studio Android SDK Manager](https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk) to make sure the following SDK components are installed. Older/Newer versions of the components may work as well, but have not necessarily been tested:
    * Platforms:
      * Android 8.1 - Oreo (API Level 27)
        * Android SDK Platform 27
          * This version (27) is required to build the project in its current configuration
          * Other platform versions may work; however, the flags in AndroidManifest.xml and the csproj must be updated as well. See the Android and Xamarin documentation for details.
        * Sources for Android 27 (optional)
    * Tools
      * Android SDK Tools
        * Android SDK Tools 26.1.1+
      * Android SDK Platform-Tools 27.0.1+
      * Android SDK Build-Tools 27.0.3+
      * Android Emulator 27.1.12+ (only required if you plan on using an emulator)

### Running on an emulator
1. Follow the Xamarin docs to [setup an Android Emulator](https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-emulator/)
1. Select the following run configuration (in the top-left):

    |                |                             |                   |
    | -------------- | --------------------------- | ----------------- |
    | Project        | `XRayAnalysis.Android`      |                   |
    | Configuration  | `Debug`                     |                   |
    | Platform       | `Any CPU`                   | (Only on Windows) |
    | Device         | `<emulator name>`           |                   |

1. Run the app

### Running on a physical device
1. Follow the instructions in the Android Developer Docs to [Enable Developer Options and USB Debugging](https://developer.android.com/studio/debug/dev-options.html)
1. Connect your Android tablet to your computer
1. Tap "OK" on the tablet when prompted to "Allow USB debugging" (Check the box to always allow if you want to make future development easier)
1. Select the following run configuration (in the top-left):

    |                |                             |                   |
    | -------------- | --------------------------- | ----------------- |
    | Project        | `XRayAnalysis.Android`      |                   |
    | Configuration  | `Debug`                     |                   |
    | Platform       | `Any CPU`                   | (Only on Windows) |
    | Device         | `<your device>`             |                   |

1. Run the app

## Building/Running the iOS App

### Running on a simulator
* **Note about Windows**
  * You can choose to [Remote the Simulator to Windows](https://docs.microsoft.com/en-us/xamarin/tools/ios-simulator) in which case a simulator that's remotely connected to your Mac Agent will launch on your Windows machine.
  * If you choose not to Remote the Simulator to Windows, the simulator will launch on the Mac Agent (this is only useful if you are able to interact with your Mac Agent)

1. In Visual Studio, select the following run configuration (in the top-left):

    |                |                             |                   |
    | -------------- | --------------------------- | ----------------- |
    | Project        | `XRayAnalysis.iOS`          |                   |
    | Configuration  | `Debug`                     |                   |
    | Platform       | `iPhoneSimulator`           |                   |
    | Device         | `<select any iPad>`         |                   |

1. Run the app

### Running on a physical device
1. Connect your iPad to your Mac / Mac Agent
1. Launch Xcode if it's not already running
1. On the iPad, if prompted to "Trust This Computer", select the option "Trust" and enter your iPad passcode
1. If iTunes launches, wait for it to identify your device, and then unplug and plug your device back in.
1. Follow the official Xamarin documentation to [setup a free provisioning profile](https://developer.xamarin.com/guides/ios/getting_started/installation/device_provisioning/free-provisioning/)
    * For the Product Name enter: `XRayAnalysis`
    * For the Organization Identifier enter: `com.companyname.XRayAnalysis`
1. Run the blank app you created from Xcode
    * You may be prompted to enter you Mac password to provide keychain access. When doing so, make sure to select "Always Allow", otherwise it will continuing asking you forever
    * You may receive a popup saying "Could not launch XRayAnalysis". Follow the instructions in the popup to trust the Developer App certificate, then run the app again.
1. You can close Xcode now
1. In Visual Studio, select the following run configuration (in the top-left):

    |                |                             |                   |
    | -------------- | --------------------------- | ----------------- |
    | Project        | `XRayAnalysis.iOS`          |                   |
    | Configuration  | `Debug`                     |                   |
    | Platform       | `iPhone`                    |                   |
    | Device         | `<your iPad name>`          |                   |

1. Run the app

## App Center (optional)
Microsoft's [Visual Studio App Center](https://www.visualstudio.com/app-center/) was also used to develop Mobile Chest X-Ray Analysis. App Center is a collection of services that enables developers to better manage and ship their applications on any platform. Mobile Chest X-Ray Analysis was developed using the following services from App Center:

* [App Center Build](https://docs.microsoft.com/en-us/appcenter/build/)
* [App Center Distribution](https://docs.microsoft.com/en-us/appcenter/distribution/)
* [App Center Crashes](https://docs.microsoft.com/en-us/appcenter/crashes/)
* [App Center Analytics](https://docs.microsoft.com/en-us/appcenter/analytics/)

App Center is free to try out. To get started with App Center, visit [https://appcenter.ms/](https://appcenter.ms/) to get an API key and follow the [Get Started with Xamarin](https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/xamarin) page for Xamarin.Android and Xamarin.iOS (not Xamarin.Forms). If you want to deploy to both platforms, you will need to create 2 projects in App Center for Xamarin iOS and Xamarin Android.

Mobile Chest X-Ray Analysis also uses [Build Scripts](https://docs.microsoft.com/en-us/appcenter/build/custom/scripts/) and [Environment Variables](https://docs.microsoft.com/en-us/appcenter/build/custom/variables/). Under the Xamarin.Android and Xamarin.iOS project, there is a `appcenter-pre-build.sh` file that is ran each time a build is triggered on App Center. This file simply writes the App Center API key to an environment file to be loaded into Mobile Chest X-Ray Analysis during runtime. 

App Center supports automated builds and distributions. For more information on how to set up automated builds, check out [Building Xamarin apps for iOS](https://docs.microsoft.com/en-us/appcenter/build/xamarin/ios/) and [Building Xamarin apps for Android](https://docs.microsoft.com/en-us/appcenter/build/xamarin/android/).

Installation instructions of an App Center distributed app can be found at [https://docs.microsoft.com/en-us/appcenter/distribution/installation](https://docs.microsoft.com/en-us/appcenter/distribution/installation).

Please note that the use of App Center is completely optional. Mobile Chest X-Ray Analysis will still run without it.

# Integrating AI

One of the primary objectives of this project was to demonstrate how a developer can easily integrate an AI model into a mobile Xamarin application.
This consists of three major steps:

1. Create / Train the Model
2. Deploy / Convert the Model
3. Consume the Model

#### The ChestXRay model

You can follow the instructions in the subsequent sections to train and integrate the real ChestXRay model which the app was designed for (some of the steps can also be extended to apply to other models as well).

This model takes a 224x224 RGB image as an input, and can be used to classify the likelihoods of 14 different chest conditions (it has a 14 element tensor as an output).

The model can also be used to generate Class Activation Maps (CAMs) by looking at the outputs of the final feature map (one of the internal layers) of the model. The CAMs highlight what parts of the input image the model "focused" on when making its classifications.

**IMPORTANT NOTE**

The application currently contains a **SAMPLE** model that has a similar architecture to the ChestXRay model but it does not actually perform any real classification. It is only there to demonstrate how a real model could integrate with the application.

The sample model is custom built to always output likelihoods of 1, 2, 3...13, 14 % for the 14 different conditions regardless of the input. The CAMs are also hard-coded within the model to output a pattern of four squares in the four corners of the image (Note: this is only in the TensorFlow protobuf model, not the CoreML ones.

Before training your model, you can run the app out-of-the-box to see these results.

## Create / Train the Model

To train the ChestXRay model, you can follow along this [blog post](https://blogs.technet.microsoft.com/machinelearning/2018/03/07/using-microsoft-ai-to-build-a-lung-disease-prediction-model-using-chest-x-ray-images/) which describes more about the model as well as how to train it using [AzureML](https://azure.microsoft.com/en-us/services/machine-learning-studio/).

## Deploy / Convert the Model

The `Model_Prep` directory of this repo contains several python scripts to simplify the process of converting a model trained using Keras (with a TensorFlow backend) to mobile-compatible formats (CoreML for iOS, TensorFlow Android for Android). Out-of-the-box these scripts are written for only the ChestXRay model; however, they can easily be extended to work with any model.

For the ChestXRay model, we need to generate the following:
* **TensorFlow Protobuf Model**
  * Version of the model to run on Android
  * Note: We do not need an additional model for the CAM generation on Android, since TensorFlow Android allows us to extract the output from inner layers
* **CoreML Model**
  * Version of the model to run on iOS
* **CoreML CAM Model**
  * Variant of the CoreML model for generating CAM images
  * On iOS, we currently don't support a multi-output model, so we have to use a second model for the CAM generation
* **Extracted Final Layer Weights**
  * CSV file containing the weights for the connection between the final two layers of the model
  * These weights are used with the output of the final feature map to generate the CAMs

### Python environment setup

1. Install Python for your platform (if it isn't already installed)
    * On Windows install Python 3.5 or 3.6
    * On Linux / macOS install Python 2.7
1. Optionally create and activate a [virtualenv](https://virtualenv.pypa.io/en/stable/) environment (recommended)
1. Install the required python packages:
    * Run the following commands from a terminal in the `Model_Prep` directory
    
      **Windows**
      ```
      pip3 install tensorflow keras
      pip3 install git+https://github.com/apple/coremltools
      pip3 install git+https://www.github.com/keras-team/keras-contrib.git
      ```
      **Linux / macOS**
      ```
      pip install tensorflow keras coremltools
      pip install git+https://www.github.com/keras-team/keras-contrib.git
      ```

###  Run the chestxray_deploy_model.py python script

```
python chestxray_deploy_model.py <path to saved, trained Keras model> <path to output directory>
```

* The 1st argument to the script is the path to the trained ChestXRay Keras model that has been saved either with `model.save(...)` or with `model.save_weights(...)`
* The 2nd argument to the script is the path to a directory where the generated models / weights should be saved
* This script will generate the four required files mentioned above.

## Consume the Model

### Add the models to the project

#### Shared Code
1. Replace `SampleModel-FinalLayerWeights.csv` (in `XRayAnalysis/Resources`) with your Extracted Final Layer Weights file (.csv)
1. Make sure the Build Action for this file is set to `Embedded Resource`
1. In `XRayAnalysis.Service.AI.ChestXRay.CAMProcessor` update `FinalLayerWeights` to match the name of your new file

#### Android
1. Replace `SampleModel.pb` (in `XRayAnalysis.Android/Assets`) with your TensorFlow Protobuf Model file (.pb)
1. Make sure the Build Action for this file is set to `AndroidAsset`
1. In `XRayAnalysis.Droid.Service.AI.ChestXRay.AndroidChestXRayAIClient` update `ModelConstants.AssetPath` to match the name of your new file

#### iOS
Note: Integrating the CoreML models will require having a Mac with Xcode installed

1. Compile each of the CoreML models:
    For each model run the following command:
    ```
    xcrun coremlcompiler compile <path to .mlmodel> <output folder>
    ```
    (The convention is for the output folder to have the same base-name as the .mlmodel but have the extension `.mlmodelc`)

1. Replace the `ChestXRay-empty.mlmodelc` and `ChestXRay-CAM-empty.mlmodelc` folders (in `XRayAnalysis.iOS/Resources`) with your compiled model folders
1. In `XRayAnalysis.iOS.Service.AI.ChestXRay.AndroidChestXRayAIClient` update `ModelConstants.ScoreModel.ResourceName` and `ModelConstants.CAMModel.ResourceName` to match the name of your new model folders

For more information, see the documentation on [Using CoreML in Xamarin](https://docs.microsoft.com/en-us/xamarin/ios/platform/introduction-to-ios11/coreml)

### Call the models from code

#### Android
On Android, we use TensorFlow-Android to interface with the model. TensorFlow-Android is provided as an Android Archive (AAR) containing the Java TensorFlow interface.
We created a binding library project (`TensorFlowAndroid`) for it that allows us to call the Java code from our C# Xamarin code.

The following excerpts from the `TensorFlowImageModel` (in the `XRayAnalysis.Android` project) class demonstrate how to use TensorFlow-Android:

```C#
// Load the model
AssetManager assets = ...;
string modelAssetPath = "<path to .pb file>";

// TensorFlowInferenceInterface is part of the TensorFlow-Android AAR
TensorFlowInferenceInterface tfInterface = new TensorFlowInferenceInterface(assets, modelAssetPath);

// Run the model on an input float[] of pixel data
float[] input = ...;
tfInterface.Feed(inputTensorName, input, batchSize, width, height, channels);

// Extract the outputs
int numOutputs = outputSizes.Length;

float[][] results = new float[numOutputs][];
// Iterate over each of the model outputs
for (int i = 0; i < numOutputs; i++)
{
    // Initialize an array to contain the data for the current output
    results[i] = new float[outputSizes[i]];
    // Fill the array with the actual data for that output (accessed by node name)
    tfInterface.Fetch(outputNodeNames[i], results[i]);
}

return results;
```

#### iOS
Xamarin.iOS has support for CoreML built-in.

The following excerpts from the `CoreMLImageModel` class demonstrate how to use the CoreML APIs:

We need to have a class that extends `IMLFeatureProvider`. This class serves to map an input name to data of type `MLFeatureValue` (in our case we're wrapping an `MLMultiArray`):
```C#
private class ImageInputFeatureProvider : NSObject, IMLFeatureProvider
{
    private string inputFeatureName;

    internal ImageInputFeatureProvider(string inputFeatureName)
    {
        this.inputFeatureName = inputFeatureName;
    }

    /// <summary>
    /// Gets or sets the image pixel data input to the model.
    /// </summary>
    public MLMultiArray ImagePixelData { get; set; }

    public NSSet<NSString> FeatureNames => new NSSet<NSString>(new NSString(inputFeatureName));

    public MLFeatureValue GetFeatureValue(string featureName)
    {
        if (inputFeatureName.Equals(featureName))
        {
            return MLFeatureValue.Create(this.ImagePixelData);
        }
        else
        {
            return MLFeatureValue.Create(0);
        }
    }
}
```

We can then load and run the model:
```C#
// Load the model
CoreMLImageModel mlModel = new CoreMLImageModel(modelName, inputFeatureName, outputFeatureName);

// Prepare the input
var inputFeatureProvider = new ImageInputFeatureProvider(inputFeatureName)
{
    ImagePixelData = input
};

// Run the model
MLMultiArray result = this.mlModel.GetPrediction(inputFeatureProvider, out NSError modelErr)
                                  .GetFeatureValue(outputFeatureName)
                                  .MultiArrayValue;
```

Note: CoreML works with the `MLMultiArray` instead of plain `float[]`s. `MLMultiArray`s are indexable and so can be treated similar to `float[]`s and converted to/from `float[]`s

# Troubleshooting

## Visual Studio not recognizing Android device

* Try disconnecting and reconnecting the device
* Try restarting the device

## Running the iOS Share Extension

iOS Share Extensions run in its own sandbox, independent of the containing application. Thus, the app extension and the containing app do not have access to each other's containers. To share data between the extension and the containing app, App Groups are used. (see 'Sharing Data with Your Containing App' on [Apple's App Extension Programming Guide](https://developer.apple.com/library/content/documentation/General/Conceptual/ExtensibilityPG/ExtensionScenarios.html#//apple_ref/doc/uid/TP40014214-CH21-SW1)). In this application, NSUserDefaults of the App Group is used to share data to the containing app.

**App Groups require Entitlements in your iOS App Provisioning Profile**. You also need to generate two Provisioning Profiles, one for the containing iOS app, while the other must be for the ShareExtension. **Both must have the same App group entitlement.**

To successfully run, your `Bundle IDs` must match the provisioning profiles, and you'll need to go to build settings of each project and select the appropriate provisioning profile.

The iOS extension project may not run on the simulator -- there is a issue with debugging Xamarin.iOS extension projects. (See the [issue on GitHub](https://github.com/xamarin/xamarin-macios/issues/3289#issuecomment-363168764)).
To run the extension, you will need to: 
- Add `--mono:static` to the additional *mtouch* arguments in all projects' iOS Build options.
- Uncheck `Enable Profiling` on the `iOS Debug` menu in the Share Extension iOS project.

## General unknown errors

In general, if you encounter unknown errors while running/debugging the app they will often be resolved by some or all of the following steps:
1. Clean the project you are trying to run
1. Clean the entire solution
1. Delete the `bin` and `obj` folders from the project you are trying to run
1. Restart Visual Studio

# Disclaimer

This model/application is intended for research and development use only. The model/application is not intended for use in clinical diagnosis or clinical decision-making or for any other clinical use and the performance of the model/application for clinical use has not been established

# License

This project is released under the MIT License.

See [LICENSE](LICENSE) for details.

# Contribute

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Acknowledgements

**Design Intern**
* Michaela Tsumura [(Linkedin)](https://www.linkedin.com/in/michaelatsumura/)

**Program Manager Intern**
* Charmaine Lee [(Linkedin)](https://www.linkedin.com/in/charmaineklee/)

**Developer Interns**
* Brendan Kellam [(Linkedin)](https://www.linkedin.com/in/brendan-kellam/)
* Jacky Lui [(Linkedin)](https://www.linkedin.com/in/jackycodes/)
* Megan Roach [(Linkedin)](https://www.linkedin.com/in/megan-c-roach/)
* Noah Tajwar [(Linkedin)](https://www.linkedin.com/in/noahtajwar/)
* Robert Lee [(Linkedin)](https://www.linkedin.com/in/robert-k-lee/)

**Coaches**
* Dirga Agoes [(Linkedin)](https://www.linkedin.com/in/dirga/)
* Robert Balent [(Linkedin)](https://www.linkedin.com/in/balent/)
* Willy-Peter Schaub [(Linkedin)](https://www.linkedin.com/in/willy-peter-schaub-aa4922/)
* Yue Ou [(Linkedin)](https://www.linkedin.com/in/yueou/)

**Program Manager**
* Stephane Morichere-Matte [(Linkedin)](https://www.linkedin.com/in/stephanemoricherematte/)

**Technical Advisor**
* Mark Schramm [(Linkedin)](https://www.linkedin.com/in/markschramm/)

