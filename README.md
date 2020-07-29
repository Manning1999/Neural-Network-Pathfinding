# Neural-Network-Pathfinding
This is a project which contains two scene, the first is a simple track which an AI has to learn to navigate and uses raycasts as it's sight, the second is a much larger scale scene which has a racecar that uses a camera for it's sight.

The racecar's neural network uses a 56x56 pixel image which it grayscales and uses for the input layer. The output layer has threee outputs; turn left, go forward and turn right. Whichever option has the highest value is the option that will be chosen. 

To train the neural network for both scenes I used an asexual evolutionary algorithm. This type of evolutionary algorithm mimics real life evolutionary processes in order to evolve the network into a workable model over multiple "generations." The evolutionary algorithm involves starting off with multiple neural networks with completely randomized weights (Randomized DNA for genetic diversity). Each network does it's own thing and is given a fitness score. The most fit network then reproduces by having it's weights copied across to all the other networks and is mutated slightly to once again have some genetic diversity. Again, each network is given a fitness score and the most fit get's to reproduce. This process is then repeated until we have a working model.

To save the models I would serialize their NeuralNetwork class and save it to my custom .NNE file

Currently, the racecars only manage their rotation but I eventually want to train them to also control their acceleration.



The scenes can be found in Assets/Scenes/

Pre-trained models can be found in Assets/Resources/Neural Networks
