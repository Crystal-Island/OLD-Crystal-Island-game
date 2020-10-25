using System;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public enum StoryType {
		FEATURE = 1,
		BUG = 2,
		CHORE = 3,
		RELEASE = 4,
	}
}