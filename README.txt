This is a line-by-line port of an open source CoverFlow implementation
for the sake of the users on #monotouch.   The original code is from
Alex Fajkowski and was originally published here:

     http://github.com/thefaj/OpenFlow

The code is released under the same terms as the original
implementation, MIT X11.

All kudos should go to him, and all bugs in this implementation are 
probably my fault - Miguel.

There is a patch included to make Flickr.NET build with MonoTouch, as
well as a ready-to-run DLL compiled like this:

	smcs -target:library -out:flickrnet.dll *.cs

What follows is Alex Fajkowski's original README file

-----
Follow the project's status at http://apparentlogic.com/openflow and on Twitter at @openflow


When I released my first iPhone app, Presenter, two months ago, Apple accused me of using their private iPhone CoverFlow API. After appealing to Apple & writing a blog post about my roadblock, Apple reversed their decision.

The initial release is simple, but it is also efficient and very fast, even on first generation iPhones.

The API should be easy to include in your own program. The main class, AFOpenFlowView, is a subclass of UIView.
To use this in your own project:
-Add the OpenFlow source code to your project.
-Add the QuartzCore and CoreGraphics frameworks.
-Import “AFOpenFlowView.h” & interact with it as you would a normal UIView.
-You should implement both the AFOpenFlowViewDelegate and AFOpenFlowViewDataSource protocols.

Currently, the delegate protocol is used to let your code know when the user selected a new object. The datasource protocol is called when AFOpenFlowView needs a UIImage object. This method should be *fast*. Don’t do NSURL requests or even disk access in this method. See the AFOpenFlowDemo app I provided for an example how to load images from a remote server. The datasource also needs to provide a default UIImage.

At any point, you can set UIImage’s on your AFOpenFlowView. You don’t need to wait for the datasource protocol to ask you. Your AFOpenFlowView will start displaying images as soon as you call setNumberOfImages.

This is an initial release of OpenFlow. I licensed it under the liberal MIT open source license.
Please drop me a line to let me know what you think & where you want the project to go from here.

The source code for both OpenFlow and the AFOpenFlowDemo project are currently available on Github at http://github.com/thefaj/OpenFlow. For the demo application, you will need to register for a Flickr API key and secret. If you try to compile the demo, you will quickly find the error message where your Key & Secret should be added.

Enjoy!
-Alex
