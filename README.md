Based on an idea from someone on Tumblr.

Pronounced "Kertho Pod".

Name comes from the Welsh for music "cerddoriaeth" (kerthor-ee-aeth) and pod, for well, pod.

## Terms
- Mixer = person who made a mix file
- Listener = person receiving and listening to a mix file
- MixFile = compressed file containing the music files, .mpdata file and optionally a UI-Static program and .mpview file

## Structure
The project is split into two programs:
 - UI-Static
 - UI-Portable

### UI-Static
The intention is that UI-Static is the first main task of this project and largely serves as a prototype. It'll be a program downloaded by the _Mixer_ and _Listener_. The _Mixer_ produces and sends a _MixFile_ to the _Listener_. They use UI-Static to load the _MixFile_ which plays the music

### UI-Portable
This is more true to the original task of a portable music player. In an attempt to keep the program file as small as possible, currently, it's looking to be a Rust CLI program, but with fancy terminal graphics. Although this is preliminary thoughts.
With this, how it'd work instead is the _Listener_ will extract the _MixFile_ and play the containing executable that the _Mixer_ has produced and customised.
