using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public class ApiError {
		[JsonProperty("code")]
		public string Code;
		[JsonProperty("error")]
		public string Error;
		[JsonProperty("general_problem")]
		public string GeneralProblem;
		[JsonProperty("validation_errors")]
		public List<ValidationError> ValidationErrors;

		public string ToHumanReadableString() {
			StringBuilder buf = new StringBuilder();
			buf.AppendLine(this.Error);
			buf.AppendLine(this.GeneralProblem);
			if (this.ValidationErrors != null && this.ValidationErrors.Count > 0) {
				buf.AppendLine("Validation errors:");
				foreach (ValidationError ve in this.ValidationErrors) {
					buf.AppendFormat("{0}: {1}\n", ve.Field, ve.Problem);
				}
			}

			return buf.ToString();
		}

		public override string ToString() {
			return String.Format("ApiError(code={0}, overview='{1}', detail='{2}')", this.Code, this.Error, this.GeneralProblem);
		}
	}

	[Serializable]
	public class ValidationError {
		[JsonProperty("field")]
		public string Field;
		[JsonProperty("problem")]
		public string Problem;

		public ValidationError() {
			this.Field = String.Empty;
			this.Problem = String.Empty;
		}

		public override string ToString() {
			return String.Format("ValidationError(field={0}, problem='{1}')", this.Field, this.Problem);
		}
	}
}