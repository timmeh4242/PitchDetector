# Overview
PitchDetector is a simple plugin for Unity for detecting the frequency of an audio source (ie, judging a player's accuracy in a karaoke game).

For now, it's a simple experiment and largely just a vehicle for getting used to Unity's new ECS + Jobs framework. I've added the auto-correlation pitch detection method from [PitchTracker](https://archive.codeplex.com/?p=pitchtracker), and then stumbled on the [TarosDSP](https://github.com/JorenSix/TarsosDSP) project; being that it was Java, I thought it'd be fun to bring over its FastYin implementation :)

# Example
Setup is simple, just put GameObjectEntity, PitchDetector, and AudioSource components on your gameObject and hit `Play`. You can open the `Entity Debugger` window to enable / disable the different systems. There's a demo scene included with the project for testing and for helping those who are truly stuck.

# What's Next
I'm not much for multi-threaded code, so for now it's all been commented out of FastYin, though performance is already quite good there. The plan is to eventually jobify + burst compile all implementations to get used to Unity's system and see what kinds of speedups we can achieve.
