Step 1: Set up C# in Visual Studio Code
Install .NET SDK:

Download and install the .NET SDK from the official website: https://dotnet.microsoft.com/download
Verify the installation by running the following command in the terminal:

Tools and Libraries:
Visual Studio (for C# development)
iText7 or PDFsharp (for PDF processing)
Magick.NET (for converting PDF to image)
Windows Forms (for building the GUI)
Step 1: Set up your C# project
Open Visual Studio.
Create a new Windows Forms App project.
Select .NET Framework as the target framework (choose .NET Core if preferred).
Add required libraries via NuGet Package Manager:
iText7 (for PDF handling) or PDFsharp.
Magick.NET-Q8-AnyCPU (for converting PDF pages to images).
To install via NuGet:

Right-click on the project > Manage NuGet Packages.
Search for Magick.NET-Q8-AnyCPU and install it.
Install either iText7 or PDFsharp for PDF handling.
Step 2: Design the Windows Forms GUI
Open the Form1.cs [Design] file in Visual Studio.
Drag and drop the following controls from the toolbox:
Label (for "Drag and Drop PDFs or Select Files")
ListBox (for listing the selected PDFs)
Buttons (for "Select PDFs", "Remove Selected", "Clear All", "Convert to PNG")
TrackBar (for DPI adjustment)
ProgressBar (for progress indication)
Set the properties for each control (e.g., button text, initial DPI value on TrackBar).
Here’s how your form might look:

plaintext
Copy code
--------------------------------------
| Label: "Drag and Drop PDFs..."     |
|------------------------------------|
| ListBox (PDF files)                |
|------------------------------------|
| Button: "Select PDFs" | Button: "Remove" |
| Button: "Clear All"   | Button: "Convert" |
|------------------------------------|
| Label: "DPI" | TrackBar            |
|------------------------------------|
| ProgressBar                        |
--------------------------------------


dotnet --version
This should print the version number of the installed SDK.
Steps to Build and Package:
Once you've modified your .csproj, you can follow these steps to generate the single-file executable:

Clean the Solution (optional but recommended):

dotnet clean
Publish the Project as a Single Executable:

dotnet publish -c Release
This command will automatically apply the settings you've added to the .csproj file, such as creating a single-file executable and including all dependencies and the .NET runtime.
Output Location:
Once the publish command completes, you can find the executable in the following directory:

/bin/Release/net8.0-windows/win-x64/publish/

1. Publishing as a Single-File Executable
You can create a single .exe file by using the dotnet publish command with the right parameters. This will bundle the executable, its dependencies, and even the .NET runtime into one file.

Here’s the command:

dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
Explanation:
-c Release: Builds the project in Release mode (optimized).
-r win-x64: Targets a 64-bit Windows platform (you can change the platform, e.g., linux-x64 for Linux).
-p:PublishSingleFile=true: Packs all the application code, dependencies, and the .NET runtime into a single file.
-p:SelfContained=true: Includes the .NET runtime so that the app can run on any machine without requiring the .NET runtime to be pre-installed.
After running this, your publish folder should contain a single .exe file.


#Dependencise 
dotnet add package Magick.NET-Q16-AnyCPU
dotnet add package itext7

#Updated .csproj File:
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Settings for single-file and self-contained publishing -->
    <PublishSingleFile>true</PublishSingleFile>       <!-- Enables single-file executable -->
    <SelfContained>true</SelfContained>               <!-- Includes the .NET runtime in the .exe -->
    <PublishTrimmed>false</PublishTrimmed>            <!-- Optional: Disable trimming for safety -->
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>    <!-- Target Windows 64-bit architecture -->
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract> <!-- Handles native libraries like Magick.NET -->
    <StripSymbols>true</StripSymbols>                 <!-- Reduces the size of the output file -->
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="itext7" Version="8.0.5" />
    <PackageReference Include="Magick.NET-Q16-AnyCPU" Version="14.0.0" />
  </ItemGroup>

</Project>
Explanation of New Properties:
<PublishSingleFile>true</PublishSingleFile>: Ensures the output is a single .exe file.
<SelfContained>true</SelfContained>: Ensures the .NET runtime is included, so the application will work on any system without .NET installed.
<PublishTrimmed>false</PublishTrimmed>: Trimming is disabled here to avoid issues with removing dependencies that are required (especially dynamic libraries like Magick.NET). You can enable trimming later, but it's safer to start with it disabled.
<RuntimeIdentifier>win-x64</RuntimeIdentifier>: This ensures the app is built for 64-bit Windows systems. You can change this to other platforms as needed (e.g., linux-x64).
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>: Ensures native libraries like Magick.NET are handled properly by extracting them at runtime.
<StripSymbols>true</StripSymbols>: Removes debugging symbols to reduce the file size.

1. Set the Application Icon (Executable Icon):
2. Add the Icon to Your Project:
Place your .ico file in a directory within your project (e.g., Assets\Icon.ico).
Modify Your .csproj: Add the following line to your .csproj inside the <PropertyGroup> to reference the icon:
<PropertyGroup>
  <ApplicationIcon>Assets\Icon.ico</ApplicationIcon>
</PropertyGroup>
3. Access the Embedded Icon: In your MainForm class, load the icon from the embedded resource:

public MainForm()
{
    // Set the taskbar and window icon from embedded resource
    this.Icon = new Icon(GetType(), "Namespace.Assets.Icon.ico");  // Replace "Namespace" with your actual namespace

    // Other initialization code
}

