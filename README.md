# What is NunitCustomAddon
Nunit Custom Addon library is a helper to extract the Test Results if you are using the NUnit and C#. At the End of complete run you would like to capture the Test Results with out doing additional code then you can use the Addon I have created for the same purpose. You can read more about the concept in [official documentation Event Listeners](https://docs.nunit.org/articles/nunit-engine/extensions/creating-extensions/Event-Listeners.html)

### How to implement your own NUnit Event Listener

#### Step 1
To use a custom NUnit Event Listener with a .addins file in your test project, you’ll need to follow these steps:

Create the Event Listener: Implement the NUnit.Engine.ITestEventListener interface in a separate class library project. This is where you’ll define what happens when test events occur.

```
using NUnit.Engine;
using NUnit.Engine.Extensibility;

[Extension]
public class MyTestListener : ITestEventListener
{
    public void OnTestEvent(string report)
    {
        // Handle the event here, e.g., log it to a file or console
        Console.WriteLine("Event data: " + report);
    }
}
```
#### Step 2
Create the .addins File: This file should be in the same directory as your test runner (e.g., nunit3-console.exe) and should contain the path to your custom listener DLL. The content of the .addins file will simply be:
```
Nunit-CustomAddon.dll
```

#### Step 3
Reference the Listener in Your Test Project: Ensure that your test project references the custom event listener project or the compiled DLL.

#### Step 4
Run Your Tests: Use the NUnit console runner to execute your tests. The runner will automatically load and use your custom event listener.

!!! Tip "Remember" 
     The .addins file is used by the NUnit engine to locate and load engine extensions1. It’s important that the file paths and names are correct and that the .addins file is placed in the correct location relative to the NUnit engine.

## How to Use Nunit-CustomAddon
1. Search for the Nuget Package with name `Nunit-CustomAddon` and add as a dependency in the you Test project.

2. Add [Nunit-CustomAddon.addins](./Addins/Nunit-CustomAddon.addins) file to the local base directory.

3. Add the .addins file reference in the project file so that its copied on the running bin like below:
```
  <ItemGroup>
    <None Update="Nunit-CustomAddon.addins">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>
```
4. Validate if the .addins file above is copied and present in the test project rinning Bin Directory where all the Test and dependency DLL's are generated.

5. Create a class example GlobalSetup below and Create the OneTimeTearDown method like below. This will be called at the end of Test Run and then you get the List of Test Results which you can use anywhere. 
>  The SetUpFixture attribute is specifically designed to work within the same assembly as your test classes. NUnit looks for classes marked with this attribute within the assembly being executed.
```
[SetUpFixture]
public class GlobalSetup
{
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (!CustomTestEventListener.allTestsFinished.WaitOne(TimeSpan.FromSeconds(10)))
        {
            TestContext.WriteLine("Timeout waiting for all tests to finish.");
        }
        //All Results are here
        List<TestResultCustom> results =  CustomTestEventListener.TestResults;
        
        // Do what you want with the Results now
    }
}
```
**Setup**:
- Uses [SetUpFixture] to ensure that setup and teardown methods run once for the entire test run.
- [OneTimeTearDown] waits for allTestsFinished to be set, with a timeout to avoid infinite waiting.
- Access CustomTestEventListener.TestResults to perform any final actions after all tests and events are processed.

**Timeout Handling**:
Add a timeout to avoid blocking indefinitely if the allTestsFinished event is not set and Proceed with the next steps.

## What is TestResultCustom Object in Response
The `TestResultCustom` class is designed to encapsulate the results of a test case execution. Its purpose is to provide a structured way to store and access the details of a test, such as its name, associated TestRail ID, outcome, start and end times, duration, and any descriptive or error information. This makes it easier to handle test data programmatically and allows for straightforward serialization to JSON format, which can be useful for logging, reporting, or interfacing with other systems that consume test results.

Here’s a breakdown of its purpose:

**TestCaseName**: Identifies the test case by name.  
**TestRailId**: Links the test result with a specific ID in TestRail, a test management tool.  
**Outcome**: Indicates whether the test passed, failed, or had another result.  
**StartTime/EndTime**: Records when the test began and ended, which can be used to track test execution over time.  
**Duration**: Measures how long the test took to execute, which can be important for performance analysis.  
**Description**: Provides additional details about what the test case covers.  
**FailureMessage**: Offers insight into why a test failed, if applicable.  
**StackTrace**: Helps with debugging by showing the call stack if a test fails.  
