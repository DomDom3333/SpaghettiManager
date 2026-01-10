# Documentation for Selected Components

## .NET MAUI
_Microsoft Application User Interface Library_

* [Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
* [GitHub](https://github.com/dotnet/maui)

## Shiny Mobile

_A cross platform framework designed to make working with device services and background processes easy, testable, and consistent while bringing things like dependency injection & logging in a structured way to your code! - Written by Allan Ritchie_

* [Documentation](https://shinylib.net/)
* [GitHub](https://github.com/shinyorg/shiny)

## Shiny Extensions

_A collection of extensions to the Shiny framework that provide additional functionality and services. These extensions are designed to enhance the capabilities of dependency injection, reflection, and application state._

* [Dependency Injection](https://shinylib.net/extensions/di/)
* [App Stores](https://shinylib.net/extensions/stores/)
* [Reflector](https://shinylib.net/extensions/reflector/) - Reflection Source Generator - NOT installed by default

## Shiny Mediator

_A simple mediator pattern for .NET applications - Written by Allan Ritchie_

* [GitHub](https://github.com/shinyorg/mediator)
* [Documentation](https://shinylib.net/client/mediator/)
* [Quick Start](https://shinylib.net/client/mediator/quick-start/)

## Community Toolkit MVVM

The CommunityToolkit.Mvvm package (aka MVVM Toolkit, formerly named Microsoft.Toolkit.Mvvm) is a modern, fast, and modular MVVM library. It is part of the .NET Community Toolkit and is built around the following principles:

Platform and Runtime Independent - .NET Standard 2.0, .NET Standard 2.1 and .NET 6 🚀 (UI Framework Agnostic)
Simple to pick-up and use - No strict requirements on Application structure or coding-paradigms (outside of 'MVVM'ness), i.e., flexible usage.
À la carte - Freedom to choose which components to use.
Reference Implementation - Lean and performant, providing implementations for interfaces that are included in the Base Class Library, but lack concrete types to use them directly.

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
* [GitHub](https://github.com/CommunityToolkit/dotnet)


## ReactiveUI

ReactiveUI is a composable, cross-platform model-view-viewmodel framework for all .NET platforms that is inspired by functional reactive programming, which is a paradigm that allows you to abstract mutable state away from your user interfaces and express the idea around a feature in one readable place and improve the testability of your application.

* [Documentation](https://reactiveui.net)
* [GitHub](https://github.com/reactiveui/ReactiveUI)

## MAUI Community Toolkit

_A collection of reusable elements for application development with .NET MAUI, including animations, behaviors, converters, effects, and helpers. It simplifies and demonstrates common developer tasks when building iOS, Android, macOS and WinUI applications._

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)
* [GitHub](https://github.com/CommunityToolkit/Maui)

## Uranium UI

_Uranium is a Free & Open-Source UI Kit for .NET MAUI. It provides a set of controls and utilities to build modern applications. It is built on top of the .NET MAUI infrastructure and provides a set of controls and layouts to build modern UIs. It also provides infrastructure for building custom controls and themes on it._  Created by Enis Necipoglu

* [Documentation](https://enisn-projects.io/docs/en/uranium/latest)
* [GitHub](https://github.com/enisn/UraniumUI)

## Skeleton for Xamarin and MAUI apps

The Skeleton control is a popular approach to loading content in mobile apps that provides one or more visual placeholders while content is being loaded. This technique is particularly useful for improving user experience, as it reduces perceived load times and provides a more engaging experience.  By Horus Software

* [GitHub](https://github.com/HorusSoftwareUY/Xamarin.Forms.Skeleton)

## CardsView MAUI

_CardsView is a view for presenting a stack of cards in a carousel-like view. It supports swiping to dismiss, dragging and dropping, and more. - Written by Andrei Misiukevich_

* [GitHub](https://github.com/AndreiMisiukevich/CardView.MAUI)

## MAUI Community Toolkit - Media Element

MediaElement is a view for playing video and audio in your .NET MAUI app.

* [Documentation](https://learn.microsoft.com/en-ca/dotnet/communitytoolkit/maui/views/mediaelement)
* [GitHub](https://github.com/CommunityToolkit/Maui)

## MAUI Community Toolkit - Camera View

The CameraView provides the ability to connect to a camera, display a preview from the camera and take photos. The CameraView also offers features to support taking photos, controlling the flash, saving captured media to a file, and offering different hooks for events.

* [Documentation](https://learn.microsoft.com/en-ca/dotnet/communitytoolkit/maui/views/camera-view)
* [GitHub](https://github.com/CommunityToolkit/Maui)

## .NET MAUI Community Toolkit - C# Markup

_The C# Markup Extensions for .NET MAUI Community Toolkit is a set of extensions that allow you to write XAML in C#._

* [Documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/markup/markup)
* [GitHub](https://github.com/CommunityToolkit/Maui)

## Native Barcode Scanner (MAUI)

_Native barcode scanning using ML and on-device APIs_

* [GitHub](https://github.com/afriscic/BarcodeScanning.Native.Maui)

## MAUI Audio Plugin

_Provides the ability to play audio inside a .NET MAUI application. - Written by Gerald Versluis_

* [GitHub](https://github.com/jfversluis/Plugin.Maui.Audio)

## MAUI OCR Plugin

_Provides the ability to perform OCR (Optical Character Recognition) on images in your .NET MAUI app - Written by Kori Francis_

* [GitHub](https://github.com/kfrancis/ocr)

## SQLite .NET PCL

_SQLite-net is an open source, minimal library to allow .NET, .NET Core, and Mono applications to store data in SQLite 3 databases - Written by Frank Krueger_

[GitHub](https://github.com/praeclarum/sqlite-net)

## SkiaSharp

_SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's Skia Graphics Library (skia.org). It provides a comprehensive 2D API that can be used across mobile, server and desktop models to render images._

* [GitHub](https://github.com/mono/SkiaSharp)
* [Skia](https://skia.org/docs/)

## SkiaSharp Extended MAUI Controls

_SkiaSharp.Extended is a collection some cool libraries that may be useful to some apps.  This package includes support for Lottie in your MAUI apps_

* [LottieFiles](https://lottiefiles.com/)
* [GitHub](https://github.com/mono/SkiaSharp.Extended)

Add the following to your XAML declaration

> xmlns:skia="clr-namespace:SkiaSharp.Extended.UI.Controls;assembly=SkiaSharp.Extended.UI"

```xml
<skia:SKLottieView
      RepeatCount="-1"
      RepeatMode="Reverse"
      Source="Girl.json" 
      HeightRequest="400"
      WidthRequest="400" />
```
## FFImageLoading
_Fast & Furious Image Loading for .NET MAUI_

Forked from the amazingly popular original FFImageLoading Library, this Compat version FFImageLoading.Compat aims to ease your migration from Xamarin.Forms to .NET MAUI with a compatible implementation to get you up and running without rewriting the parts of your app that relied on the original library.

This Maui version which merges all Transformations & SVG library parts into ONE and is migrated from FFImageLoading.Compat aims to fix some critical bugs and gives you a place to submit Maui releated issues.

The Google webp format image support. (It works in Xamarin.Forms version, but not in FFImageLoading.Compat)
Thanks to the Original Authors: Daniel Luberda, Fabien Molinet & Redth.

* [GitHub](https://github.com/microspaze/FFImageLoading.Maui)

## ACR User Dialogs

_A cross platform library that allows you to call for standard user dialogs from a shared/portable library that supports Android & iOS by Allan Ritchie_

* [GitHub](https://github.com/aritchie/userdialogs)

## AlohaKit Animations

_AlohaKit.Animations is a library designed for .NET MAUI that aims to facilitate the use of animations to developers. Very simple use from C# and XAML code by Javier Suárez_

* [GitHub](https://github.com/jsuarezruiz/AlohaKit.Animations)

## Settings View

_This is a flexible TableView specialized in settings for Android / iOS by Satoshi NaKamura_

* [GitHub](https://github.com/muak/AiForms.Maui.SettingsView)

## System.Linq.Async

IAsyncEnumerable<T>, and the associated version C# (8.0) added intrinsic support for this interface with its await foreach construct.

Although .NET Core 3.0 defined IAsyncEnumerable<T>, it did not add any corresponding LINQ implementation. Whereas IEnumerable<T> supports all the standard operators such as Where, GroupBy, and SelectMany, .NET does not have built-in implementations of any of these for IAsyncEnumerable<T>. However, Ix had provided LINQ operators for its prototype version of IAsyncEnumerable<T> from the start, so when .NET Core 3.0 shipped, it was a relatively straightforward task to update all those existing LINQ operators to work with the new, official IAsyncEnumerable<T>.

Thus, the System.Linq.Async NuGet package was created, providing a LINQ to Objects implementation for IAsyncEnumerable<T> to match the one already built into .NET for IEnumerable<T>.

Since all of the relevant code was already part of the Ix project (with IAsyncEnumerable<T> also originally having been defined by this project), the System.Linq.Async NuGet package is built as part of the Ix project.

* [GitHub](https://github.com/dotnet/reactive)

## Humanizer

Humanizer meets all your .NET needs for manipulating and displaying strings, enums, dates, times, timespans, numbers and quantities.

* [GitHub](https://github.com/Humanizr/Humanizer)

## Units .NET
Written by Andreas Gullberg Larsen

Add strongly typed quantities to your code and get merrily on with your life.

No more magic constants found on Stack Overflow, no more second-guessing the unit of parameters and variables.

* [GitHub](https://github.com/angularsen/UnitsNet)

