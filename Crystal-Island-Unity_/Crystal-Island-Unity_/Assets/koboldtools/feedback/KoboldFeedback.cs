using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using KoboldTools.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using KoboldTools.Feedback.Pivotal;

namespace KoboldTools.Feedback {
    public enum FeedbackType {
        NONE = 0,
        FEATURE = 1,
        BUG = 2,
    }

    [Serializable]
    public class SystemMetadata {
        [JsonProperty("product_name")]
        public readonly string ProductName;
        [JsonProperty("product_version")]
        public readonly string ProductVersion;
        [JsonProperty("device_model")]
        public readonly string DeviceModel;
        [JsonProperty("operating_system")]
        public readonly string OperatingSystem;
        [JsonProperty("unity_version")]
        public readonly string UnityVersion;
        [JsonProperty("unity_platform")]
        public readonly string UnityPlatform;
        [JsonProperty("unity_device_type")]
        public readonly string UnityDeviceType;

        public SystemMetadata() {
            this.ProductName = Application.productName;
            this.ProductVersion = Application.version;
            this.DeviceModel = SystemInfo.deviceModel;
            this.OperatingSystem = SystemInfo.operatingSystem;
            this.UnityVersion = Application.unityVersion;
            this.UnityPlatform = Application.platform.ToString();
            this.UnityDeviceType = SystemInfo.deviceName;
        }

        public override string ToString() {
            return String.Format("{0} (version: {1}, unity: {2})\nModel: {3}, OS: {4}\nPlatform: {5}, type: {6}", this.ProductName, this.ProductVersion, this.UnityVersion, this.DeviceModel, this.OperatingSystem, this.UnityPlatform, this.UnityDeviceType);
        }

        public string ToMachineReadableString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class KoboldFeedback : MonoBehaviour {
        public bool PauseGame = false;
        public bool AttachLogs = true;
        public bool IncludeSystemInfo = true;
        public bool MachineReadableLogs = false;

        [Header("Pivotal Tracker Settings")]
        public string ProjectId;
        public string ApiToken;
        public string ApiEntrypoint = "https://www.pivotaltracker.com/services/v5";

        [Header("Story Settings")]
        public List<Label> StoryLabels;
        public string StoryTitle = "Unbearbeitetes Player Feedback";

        [Header("Form References")]
        public Panel FormPanel;
        public Button submitButton;
        public Button cancelButton;
        public Dropdown storyTypeDropdown;
        public Text featureIWantLabel;
        public InputField featureIWant;
        public Text featureBecauseLabel;
        public InputField featureBecause;
        public Text bugDescriptionLabel;
        public InputField bugDescription;
        public Text bugReproductionLabel;
        public InputField bugReproduction;

        [Header("Other References")]
        public KoboldTools.Logging.Logger Logger;

        private List<GameObject> DisplayWithFeature;
        private List<GameObject> DisplayWithBug;
        private Dictionary<FeedbackType, String> FeedbackTypeLocalisations;

        private void Awake() {
            if (this.FormPanel == null) {
                this.FormPanel = this.GetComponent<Panel>();
            }
            
            this.FeedbackTypeLocalisations = new Dictionary<FeedbackType, string> {
                {FeedbackType.NONE, "feedbackDropdownLabel"},
                {FeedbackType.FEATURE, "feedbackFeatureLabel"},
                {FeedbackType.BUG, "feedbackBugLabel"},
            };

            this.DisplayWithFeature = new List<GameObject> {
                this.featureIWantLabel.gameObject,
                this.featureIWant.gameObject,
                this.featureBecauseLabel.gameObject,
                this.featureBecause.gameObject,
            };

            this.DisplayWithBug = new List<GameObject> {
                this.bugDescriptionLabel.gameObject,
                this.bugDescription.gameObject,
                this.bugReproductionLabel.gameObject,
                this.bugReproduction.gameObject,
            };
        }

        private void Start() {
            Localisation.instance.eLanguageChanged.AddListener(this.OnLanguageChanged);
            this.storyTypeDropdown.onValueChanged.AddListener(this.OnFeedbackTypeChanged);
            this.FormPanel.openComplete.AddListener(this.OnOpenForm);
            this.submitButton.onClick.AddListener(this.OnSubmitForm);
            this.cancelButton.onClick.AddListener(this.OnCancelForm);

            this.OnLanguageChanged();
            this.OnFeedbackTypeChanged(this.storyTypeDropdown.value);
        }

        private void OnLanguageChanged() {
            this.storyTypeDropdown.ClearOptions();
            foreach (FeedbackType t in Enum.GetValues(typeof(FeedbackType))) {
                string locId = this.FeedbackTypeLocalisations[t];
                this.storyTypeDropdown.options.Add(new Dropdown.OptionData(Localisation.instance.getLocalisedText(locId)));
            }
            this.storyTypeDropdown.RefreshShownValue();
        }

        private void OnFeedbackTypeChanged(int option) {
            FeedbackType t = (FeedbackType) Enum.ToObject(typeof(FeedbackType), option);
            foreach (GameObject go in DisplayWithFeature) {
                go.SetActive(t == FeedbackType.FEATURE);
            }
            foreach (GameObject go in DisplayWithBug) {
                go.SetActive(t == FeedbackType.BUG);
            }
        }

        private void OnOpenForm() {
            if (this.PauseGame) {
                Time.timeScale = 0f;
            }
            this.ClearForm();
        }

        private void OnCancelForm() {
            this.CloseForm();
        }

        private void OnSubmitForm() {
            Story s = new Story();
            s.CurrentState = StoryState.UNSCHEDULED;
            s.Name = StoryTitle;
            s.Labels = this.StoryLabels;
            FeedbackType feedbackType = (FeedbackType) Enum.ToObject(typeof(FeedbackType), storyTypeDropdown.value);

            if (feedbackType == FeedbackType.FEATURE) {
                if (String.IsNullOrEmpty(featureIWant.text) && String.IsNullOrEmpty(featureBecause.text)) {
                    return;
                }
                s.StoryType = StoryType.FEATURE;
                s.Description = String.Format("{0}\n{1}\n{2}\n{3}", featureIWantLabel.text, featureIWant.text, featureBecauseLabel.text, featureBecause.text);
            } else if (feedbackType == FeedbackType.BUG) {
                if (String.IsNullOrEmpty(bugDescription.text) && String.IsNullOrEmpty(bugReproduction.text)) {
                    return;
                }
                s.StoryType = StoryType.BUG;
                s.Description = String.Format("{0}\n{1}\n{2}\n{3}", bugDescriptionLabel.text, bugDescription.text, bugReproductionLabel.text, bugReproduction.text);
            } else {
                return;
            }

            this.submitButton.interactable = false;
            this.cancelButton.interactable = false;
            StartCoroutine(this.PivotalCreateStory(s));
        }

        private void CloseForm() {
            if (this.PauseGame) {
                Time.timeScale = 1f;
            }
            this.FormPanel.onClose();
        }

        private void ClearForm() {
            featureIWant.text = String.Empty;
            featureBecause.text = String.Empty;
            bugDescription.text = String.Empty;
            bugReproduction.text = String.Empty;
            storyTypeDropdown.value = 0;
        }

        private IEnumerator PivotalCreateStory(Story story) {
            string jsonData = JsonConvert.SerializeObject(story);
            string endpoint = String.Format("{0}/projects/{1}/stories", this.ApiEntrypoint, ProjectId);

            UTF8Encoding encoding = new UTF8Encoding();
            UnityWebRequest w = new UnityWebRequest(endpoint);
            w.uploadHandler = new UploadHandlerRaw(encoding.GetBytes(jsonData));
            w.downloadHandler = new DownloadHandlerBuffer();
            w.method = UnityWebRequest.kHttpVerbPOST;

            w.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            w.SetRequestHeader("X-TrackerToken", ApiToken);

            RootLogger.Debug(this, "Submitting story at {0}", endpoint);
            yield return w.SendWebRequest();

            if (w.isNetworkError) {
                RootLogger.Error(this, "Network error: {0}", w.error);
            } else if (w.isHttpError) {
                if (w.responseCode == 400) {
                    ApiError err = JsonConvert.DeserializeObject<ApiError>(w.downloadHandler.text);
                    RootLogger.Error(this, "API error: {0}", err.ToHumanReadableString());
                } else {
                    RootLogger.Error(this, "HTTP error {0}: {1}", w.responseCode, w.error);
                }
            } else {
                RootLogger.Debug(this, "The new story was created");
                Story committedStory = JsonConvert.DeserializeObject<Story>(w.downloadHandler.text);
                if (this.AttachLogs && this.Logger != null) {
                    yield return StartCoroutine(this.PivotalUploadLogs(committedStory));
                }
            }

            this.submitButton.interactable = true;
            this.cancelButton.interactable = true;
            this.CloseForm();
        }

        private IEnumerator PivotalUploadLogs(Story story) {
            string endpoint = String.Format("{0}/projects/{1}/uploads", this.ApiEntrypoint, ProjectId);
            StringBuilder buf = new StringBuilder();
            if (this.IncludeSystemInfo) {
                SystemMetadata m = new SystemMetadata();
                if (this.MachineReadableLogs) {
                    buf.AppendLine(m.ToMachineReadableString());
                } else {
                    buf.AppendLine(m.ToString());
                }
            }
            foreach (Record r in this.Logger.Records) {
                if (this.MachineReadableLogs) {
                    buf.AppendLine(r.ToMachineReadableString());
                } else {
                    buf.AppendLine(r.ToHumanReadableString(true, true, false, true, true));
                }
            }

            UTF8Encoding encoding = new UTF8Encoding();
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("file", encoding.GetBytes(buf.ToString()), "logs.txt", "text/plain; charset=utf-8"));
            UnityWebRequest w = UnityWebRequest.Post(endpoint, formData);
            w.SetRequestHeader("X-TrackerToken", ApiToken);

            RootLogger.Debug(this, "Submitting the log file at {0}", endpoint);
            yield return w.SendWebRequest();

            if (w.isNetworkError) {
                RootLogger.Error(this, "Network error: {0}", w.error);
            } else if (w.isHttpError) {
                if (w.responseCode == 400) {
                    ApiError err = JsonConvert.DeserializeObject<ApiError>(w.downloadHandler.text);
                    RootLogger.Error(this, "API error: {2}", err.ToHumanReadableString());
                } else {
                    RootLogger.Error(this, "HTTP error {0}: {1}", w.responseCode, w.error);
                }
            } else {
                RootLogger.Debug(this, "The log file was uploaded");
                FileAttachment committedFile = JsonConvert.DeserializeObject<FileAttachment>(w.downloadHandler.text);
                yield return StartCoroutine(this.PivotalCreateComment(story, committedFile));
            }
        }

        private IEnumerator PivotalCreateComment(Story story, FileAttachment attachment) {
            Comment cmt = new Comment();
            cmt.Text = "Uploaded client logs";
            cmt.FileAttachments = new List<FileAttachment> { attachment };

            string jsonData = JsonConvert.SerializeObject(cmt);
            string endpoint = String.Format("{0}/projects/{1}/stories/{2}/comments", this.ApiEntrypoint, ProjectId, story.Id);

            UTF8Encoding encoding = new UTF8Encoding();
            UnityWebRequest w = new UnityWebRequest(endpoint);
            w.uploadHandler = new UploadHandlerRaw(encoding.GetBytes(jsonData));
            w.downloadHandler = new DownloadHandlerBuffer();
            w.method = UnityWebRequest.kHttpVerbPOST;

            w.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            w.SetRequestHeader("X-TrackerToken", ApiToken);

            RootLogger.Debug(this, "Submitting comment at {0}", endpoint);
            yield return w.SendWebRequest();

            if (w.isNetworkError) {
                RootLogger.Error(this, "Network error: {0}", w.error);
            } else if (w.isHttpError) {
                if (w.responseCode == 400) {
                    ApiError err = JsonConvert.DeserializeObject<ApiError>(w.downloadHandler.text);
                    RootLogger.Error(this, "API error: {2}", err.ToHumanReadableString());
                } else {
                    RootLogger.Error(this, "HTTP error {0}: {1}", w.responseCode, w.error);
                }
            } else {
                RootLogger.Debug(this, "The new comment was created");
            }
        }
    }
}