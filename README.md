## What it does
ResAR is an Augmented Reality Circuit Visualizer and Solver. A user can place down circuit elements in parallel and series configurations and ResistAR will solve the current through and voltage across each element of the circuit. It gives the user an easy way to _see_ (sharp) the circuit.

## How we built it
We first began with 3D printed chassis for the [VuMark] (https://library.vuforia.com/articles/Training/VuMark) targets. These targets are identified and parsed by the program and cross checked against our cloud database on [Vuforia] (https://www.vuforia.com/). 
We then created 3D, textured, models in [Blender] (https://www.blender.org/) that will hover over the VuMark targets.
We then wrote the code in [Unity] (unity3d.com) that will calculate voltage and current values using concepts from vector calculus and matrix algebra.

## Challenges we ran into
The math was very difficult and attempting to rush a 3D printed design was also difficult but there was a rush because 3D printing would be a very time consuming process. 
Thus we also had to create a lot of our latter designs around the already 3D printed parts.
VuMarks were also difficult to create. VuMarks must be very easily distinguishable from each other and non-symmetric along any axis, and therefore took a while to get finely tuned and calibrated.
Finally the math was a very difficult thing to visualize. We had to go from 3D space to 2D space and there were some difficulties with projections. The coders did end up writing relatively bug-free code, but not before a long, arduous thinking process.

