using UnityEngine;

namespace Polymoney
{
    [RequireComponent(typeof(AnalyticsManager))]
    public class RootAnalytics : MonoBehaviour
    {
        private static AnalyticsManager Instance;

        public void Awake()
        {
            RootAnalytics.Instance = GetComponent<AnalyticsManager>();
        }

        public static void SnapshotMonth(int monthId)
        {
            if(RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SnapshotMonth(monthId);
            }
        }

        public static AnalyticsManager GetAnalyticsManager()
        {
            if(RootAnalytics.Instance != null)
            {
                return RootAnalytics.Instance;
            }
            return null;
        }

        public static int GetMaxBalance()
        {
            if(RootAnalytics.Instance != null)
            {
                return RootAnalytics.Instance.GetMaxBalance();
            }
            return -1;
        }

        public static void AddPlayer(uint playerId)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.AddPlayer(playerId);
            }
        }

        public static void SetPlayerEnjoyment(uint playerId, int value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetPlayerEnjoyment(playerId, value);
            }
        }

        public static void SetAccountBalance(uint playerId, Currency currency, int value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetAccountBalance(playerId, currency, value);
            }
        }

        public static void AddWelfarePayment(uint playerId, int value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.AddWelfarePayment(playerId, value);
            }
        }

        public static void AddOffer(uint playerId, Currency currency)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.AddOffer(playerId, currency);
            }
        }

        public static void SetFoodHealthStatus(uint playerId, float status)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetFoodHealthStatus(playerId, status);
            }
        }

        public static void SetCirculationRate(Currency currency, int value, float timeFrame)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetCirculationRate(currency, value, timeFrame);
            }
        }

        public static void SetTalentNumber(uint playerId, int talents)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetTalentNumber(playerId, talents);
            }
        }

        public static void SetCityAccountBalance(int value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.SetCityAccountBalance(value);
            }
        }

        public static void UpdateCityState(float value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.UpdateCityState(value);
            }
        }

        public static void UpdateCityLuminance(float value)
        {
            if (RootAnalytics.Instance != null)
            {
                RootAnalytics.Instance.UpdateCityLuminance(value);
            }
        }
    }
}
