using System;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public enum StoryState {
		ACCEPTED = 1,
		DELIVERED = 2,
		FINISHED = 3,
		STARTED = 4,
		REJECTED = 5,
		PLANNED = 6,
		UNSTARTED = 7,
		UNSCHEDULED = 8,
	}
}