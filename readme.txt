UNITY PROJECTILE PHYSICS FOLDER
This folder contains a script which simulates a trajectory of a projectile according to the laws of physics. One of the methods calculates an initial velocity vector considering the physic forces such as air resistance force.Another method precomputes and returns a list of the trajectory points with a given timestep and an initial velocity vector.

KOTLIN FOLDER
Inside this folder you can find a reduced version of a Repository connected with Local and Remote Databases developed for the personal android app (only a Profile and Profession objects represented there)

UNITY MEDIA PLAYER ENGINE FOLDER
Contains a script that represents an engine for handling a media player that runs video replays.

UNITY VOICE ASSISTANT FOLDER
One of the core features of the developed App is designed to execute users' commands via voice control.

Keywords.cs:
This is where all the keywords are stored. Dictionary has an ID and an array of keywords linked to that ID.

PhraseAnalysis.cs:
This script analyzes the result string received from the Yandex Cloud Server and provides info on which item from the Keywords has the most matches.

YandexSpeechKit.cs:
This script sends a web request consisting of an audio file recorded directly from the app to the Yandex Cloud Server.

VPBase.cs:
A base class for handling VideoPlayer events. OnYandexRespond() receives a result string  from the Yandex Cloud Server then passes it to the method from PhraseAnalysis
and executes custom logic according to the analysis results.

VPProducts.cs:
One of the derived classes with specific functionality.
