Kotlin folder
Inside this folder you can find a reduced version of a Repository connected with Local and Remote Databases developed for the personal android app (only a Profile and Profession objects represented there)
Project 1.
One of the features of the App is designed to execute users' commands via voice control.

YandexSpeechKit.cs:
This script sends a web request consisting of an audio file recorded directly from the app to the Yandex Cloud Server.

PhraseAnalysis.cs:
This script analyzes the result string received from the Yandex Cloud Server and provides info on which item from the Keywords has the most matches.

Keywords.cs:
This is where all the keywords are stored. Dictionary has an ID and an array of keywords linked to that ID.

VPBasic.cs:
A base class for handling VideoPlayer events. OnYandexRespond() receives a result string  from the Yandex Cloud Server then passes it to the method from PhraseAnalysis
and does some stuff according to that analysis.

VPProducts.cs:
One of the derived classes with specific functionality.

Project 2.
One of the game tools precalculates projectile's velocity and trajectory.

ProjectileTrajectoryPredictor.cs
This script has methods for precomputing projectile trajectory with a given start velocity vector or for calculating start projectile velocity to hit given target
