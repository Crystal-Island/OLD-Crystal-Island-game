////////////////////////////////////////////////////////////////////////////////
//  
// @module Google Analytics Plugin
// @author Osipov Stanislav (Stan's Assets) 
// @support support@stansassets.com
// @website https://stansassets.com
//
////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections.Generic;
using SA.Analytics;

namespace SA.Analytics.Google {

	public class UseExample : MonoBehaviour {

		void Start () {
			//This call will be ignored of you already have GoogleAnalytics gameobject on your scene, like in the example scene
			//However if you do not want to create additional object in your initial scene
			//you may use this code to initialize analytics
			//WARNING: if you do not have GoogleAnalytics gamobect and you skip StartTracking call, GoogleAnalytics gameobect will be
			//initialized on first GoogleAnalytics.Client call
			GA_Manager.StartTracking();

			Firebase.Analytics.Init ();
		}
		

		void OnGUI () {
			if(GUI.Button(new Rect(10, 10, 150, 50), "Page Hit")) {
				GA_Manager.Client.SendPageHit("mydemo.com ", "/home", "homepage");
			}
			
			
			if(GUI.Button(new Rect(10, 70, 150, 50), "Event Hit")) {
				GA_Manager.Client.SendEventHit("video", "play", "holiday", 300);
			}

			
			if(GUI.Button(new Rect(10, 130, 150, 50), "Transaction Hit")) {
				GA_Manager.Client.SendTransactionHit("12345", "westernWear", "EUR", 50.00f, 32.00f, 12.00f);
			}

			if(GUI.Button(new Rect(10, 190, 150, 50), "Item Hit")) {
				GA_Manager.Client.SendItemHit("12345", "sofa", "u3eqds43", 300f, 2, "furniture", "EUR");
			}

			if(GUI.Button(new Rect(190, 10, 150, 50), "Social Hit")) {
				GA_Manager.Client.SendSocialHit("like", "facebook", "/home ");
			}

			if(GUI.Button(new Rect(190, 70, 150, 50), "Exception Hit")) {
				GA_Manager.Client.SendExceptionHit("IOException", true);
			}

			if(GUI.Button(new Rect(190, 130, 150, 50), "Timing Hit")) {
				GA_Manager.Client.SendUserTimingHit("jsonLoader", "load", 5000, "jQuery");
			}

			if(GUI.Button(new Rect(190, 190, 150, 50), "Screen Hit")) {
				GA_Manager.Client.SendScreenHit("MainMenu");
			}
		
			if(GUI.Button(new Rect(10, 270, 150, 50), "Firebase Event")) {
				Dictionary<string, object> data = new Dictionary<string, object> ();
				data.Add ("str_data", "hello_from_firebase");
				data.Add ("numeric_data", 10101);
				Firebase.Analytics.LogEvent ("ga_event", data);
			}
		}

		public void CustomBuildersExamples() {

			//Page Tracking
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.SetDocumentHostName("mydemo.com");
			GA_Manager.Client.SetDocumentPath("/home");
			GA_Manager.Client.SetDocumentTitle("homepage");

			GA_Manager.Client.Send();


			//Event Tracking
			GA_Manager.Client.CreateHit(HitType.EVENT);
			GA_Manager.Client.SetEventCategory("video");
			GA_Manager.Client.SetEventAction("play");
			GA_Manager.Client.SetEventLabel("holiday");
			GA_Manager.Client.SetEventValue(300);

			GA_Manager.Client.Send();



			//Measuring Purchases
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.SetDocumentHostName("mydemo.com");
			GA_Manager.Client.SetDocumentPath("/receipt");
			GA_Manager.Client.SetDocumentTitle("Receipt Page");

			GA_Manager.Client.SetTransactionID("T12345");
			GA_Manager.Client.SetTransactionAffiliation("Google Store - Online");
			GA_Manager.Client.SetTransactionRevenue(37.39f);
			GA_Manager.Client.SetTransactionTax(2.85f);
			GA_Manager.Client.SetTransactionShipping(5.34f);
			GA_Manager.Client.SetTransactionCouponCode("SUMMER2013");

			GA_Manager.Client.SetProductAction("purchase");
			GA_Manager.Client.SetProductSKU(1, "P12345");
			GA_Manager.Client.SetSetProductName(1, "Android Warhol T-Shirt");
			GA_Manager.Client.SetProductCategory(1, "Apparel");
			GA_Manager.Client.SetProductBrand(1, "Google");
			GA_Manager.Client.SetProductVariant(1, "Black");
			GA_Manager.Client.SetProductPosition(1, 1);

			GA_Manager.Client.Send();



		

			//Measuring Refunds

			// Refund an entire transaction and send with a non-interaction event.
			GA_Manager.Client.CreateHit(HitType.EVENT);
			GA_Manager.Client.SetEventCategory("Ecommerce");
			GA_Manager.Client.SetEventAction("Refund");
			GA_Manager.Client.SetNonInteractionFlag();
			GA_Manager.Client.SetTransactionID("T12345");
			GA_Manager.Client.SetProductAction("refund");

			GA_Manager.Client.Send();


			// Refund a single product.
			GA_Manager.Client.CreateHit(HitType.EVENT);
			GA_Manager.Client.SetEventCategory("Ecommerce");
			GA_Manager.Client.SetEventAction("Refund");
			GA_Manager.Client.SetNonInteractionFlag();
			GA_Manager.Client.SetTransactionID("T12345");
			GA_Manager.Client.SetProductAction("refund");
			GA_Manager.Client.SetProductSKU(1, "P12345");
			GA_Manager.Client.SetProductQuantity(1, 1);

			GA_Manager.Client.Send();




			// Measuring Checkout Steps
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.SetDocumentHostName("mydemo.com");
			GA_Manager.Client.SetDocumentPath("/receipt");
			GA_Manager.Client.SetDocumentTitle("Receipt Page");
			
			GA_Manager.Client.SetTransactionID("T12345");
			GA_Manager.Client.SetTransactionAffiliation("Google Store - Online");
			GA_Manager.Client.SetTransactionRevenue(37.39f);
			GA_Manager.Client.SetTransactionTax(2.85f);
			GA_Manager.Client.SetTransactionShipping(5.34f);
			GA_Manager.Client.SetTransactionCouponCode("SUMMER2013");
			
			GA_Manager.Client.SetProductAction("purchase");
			GA_Manager.Client.SetProductSKU(1, "P12345");
			GA_Manager.Client.SetSetProductName(1, "Android Warhol T-Shirt");
			GA_Manager.Client.SetProductCategory(1, "Apparel");
			GA_Manager.Client.SetProductBrand(1, "Google");
			GA_Manager.Client.SetProductVariant(1, "Black");
			GA_Manager.Client.SetProductPrice(1, 29.90f);
			GA_Manager.Client.SetProductQuantity(1, 1);
			GA_Manager.Client.SetCheckoutStep(1);
			GA_Manager.Client.SetCheckoutStepOption("Visa");

			GA_Manager.Client.Send();


			//Measuring Checkout Options
			GA_Manager.Client.CreateHit(HitType.EVENT);
			GA_Manager.Client.SetEventCategory("Checkout");
			GA_Manager.Client.SetEventAction("Option");
			GA_Manager.Client.SetProductAction("checkout_option");
			GA_Manager.Client.SetCheckoutStep(2);
			GA_Manager.Client.SetCheckoutStepOption("FedEx");

			GA_Manager.Client.Send();



			//Measuring Internal Promotions

			//Promotion Impressions
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.SetDocumentHostName("mydemo.com");
			GA_Manager.Client.SetDocumentPath("/home");
			GA_Manager.Client.SetDocumentTitle("homepage");
			GA_Manager.Client.SetPromotionID(1, "PROMO_1234");
			GA_Manager.Client.SetPromotionName(1,"Summer Sale");
			GA_Manager.Client.SetPromotionCreative(1, "summer_banner2");
			GA_Manager.Client.SetPromotionPosition(1, "banner_slot1");
			
			GA_Manager.Client.Send();


			//Promotion Clicks
			GA_Manager.Client.CreateHit(HitType.EVENT);
			GA_Manager.Client.SetEventCategory("Internal Promotions");
			GA_Manager.Client.SetEventAction("click");
			GA_Manager.Client.SetEventLabel("Summer Sale");
			GA_Manager.Client.SetPromotionAction("click");
			GA_Manager.Client.SetPromotionID(1, "PROMO_1234");
			GA_Manager.Client.SetPromotionName(1,"Summer Sale");
			GA_Manager.Client.SetPromotionCreative(1, "summer_banner2");
			GA_Manager.Client.SetPromotionPosition(1, "banner_slot1");

			
			GA_Manager.Client.Send();
			

			//Ecommerce Tracking



			//Transaction Hit
			GA_Manager.Client.CreateHit(HitType.TRANSACTION);
			GA_Manager.Client.SetTransactionID("12345");
			GA_Manager.Client.SetTransactionAffiliation("westernWear");
			GA_Manager.Client.SetTransactionRevenue(50);
			GA_Manager.Client.SetTransactionShipping(32f);
			GA_Manager.Client.SetTransactionTax(12f);
			GA_Manager.Client.SetCurrencyCode("EUR");

			GA_Manager.Client.Send();



			//Item Hit
			GA_Manager.Client.CreateHit(HitType.ITEM);
			GA_Manager.Client.SetTransactionID("12345");
			GA_Manager.Client.SetItemName("sofa");
			GA_Manager.Client.SetItemPrice(300f);
			GA_Manager.Client.SetItemQuantity(2);
			GA_Manager.Client.SetItemCode("u3eqds43");
			GA_Manager.Client.SetItemCategory("furniture");
			GA_Manager.Client.SetCurrencyCode("EUR");

			GA_Manager.Client.Send();

				
			//Social Interactions
			GA_Manager.Client.CreateHit(HitType.SOCIAL);     
			GA_Manager.Client.SetSocialAction("like");     				
			GA_Manager.Client.SetSocialNetwork("facebook"); 
			GA_Manager.Client.SetSocialActionTarget("/home  ");

			GA_Manager.Client.Send();


			//Exception Tracking
			GA_Manager.Client.CreateHit(HitType.EXCEPTION);  
			GA_Manager.Client.SetExceptionDescription("IOException");
			GA_Manager.Client.SetIsFatalException(true);

			GA_Manager.Client.Send();



			//User Timing Tracking
			GA_Manager.Client.CreateHit(HitType.TIMING); 
			GA_Manager.Client.SetUserTimingCategory("jsonLoader");
			GA_Manager.Client.SetUserTimingVariableName("load");
			GA_Manager.Client.SetUserTimingTime(5000);
			GA_Manager.Client.SetUserTimingLabel("jQuery");

			GA_Manager.Client.SetDNSTime(100);
			GA_Manager.Client.SetPageDownloadTime(20);
			GA_Manager.Client.SetRedirectResponseTime(32);
			GA_Manager.Client.SetTCPConnectTime(56);
			GA_Manager.Client.SetServerResponseTime(12);

			GA_Manager.Client.Send();






			//Custom builder example

			//Simple Page hit
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.SetDocumentHostName("mydemo.com");
			GA_Manager.Client.SetDocumentPath("/home");
			GA_Manager.Client.SetDocumentTitle("homepage");
			
			GA_Manager.Client.Send();
			
			//Constructing Same page hit without plugin methods
			GA_Manager.Client.CreateHit(HitType.PAGEVIEW);
			GA_Manager.Client.AppendData("dh", "mydemo.com");
			GA_Manager.Client.AppendData("dp", "/home");
			GA_Manager.Client.AppendData("dt", "homepage");

			GA_Manager.Client.Send();

		


		}
	}

}
