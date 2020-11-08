# Graphic Tests

You can find the graphic tests for mixture in the [Test Runner Window](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html) under the **PlayMode** tab.

![image](https://user-images.githubusercontent.com/6877923/91179929-74d26200-e6e7-11ea-84a4-e307a9fa714b.png)

It's a good practice to run these tests and ensure they are green before submitting a code modification. When adding a new node, it's also recommended to add a new test in the correct category to ensure your work won't break when someone else modify the code.

:warning: note that currently the tests can only run inside the editor and thus the **Run all in player** button doesn't work.

## Adding a new test

To add a new graphic test you must have cloned this repository and then follow these steps:
- Create a new `🎨 Static Mixture Graph` in one of the folder in `Assets/GraphicTests/Mixtures/` depending on the type of test you want to add. For example a noise test will go inside the `00_Simple` folder and will be named `00xx_TestName` (replace x by the highest test number in the folder + 1).
- Make the test graph you need (don't forget to lower the output resolution to LDR and enable compression to avoid pushing big images).
- Then you need to refresh the list of tests in the `Test Runner Window`, you can do so by entering and exiting play mode.
- At this step you should be able to see your new test in the list. Double click on your test to run it.
- Once your test is finished, it should have failed with this kind of error message:

```
No reference image found for x (Mixture.MixtureGraph), Creating one at ...
Please re-run the test to ensure the reference image validity.
```

- As stated above, the graphic test runner automatically took the result image of your graph (located in the `ActualImages` folder of the project) and copied it to the `ReferenceImages` folder. Now all you have to do is re-run the test one more time.
- The test should now be green and you can push it :)

## Updating a reference image

In case you changed the code of a node and result in an expected failure of the graphic tests, you'll need to update the reference images. Here's how to do it:

- First run your test in the graphic test window.
- Then go to the `Assets/ActualImages/WindowsEditor/Direct3D11/None/` folder (on mac the path is different) and refresh the `Project Window` view (ctrl-R or right click -> refresh) to show the result images.
- Pick the correct image which should be called `MixtureTests(00xx_TestName).png` and move it to the `Asset/GraphicTests/ReferenceImages/` folder.
- Finally rename the image with the name of the test which means replacing the old image by the new one.