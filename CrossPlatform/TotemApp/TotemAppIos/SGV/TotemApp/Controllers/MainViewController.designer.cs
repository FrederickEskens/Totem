// WARNING
//
// This file has been generated automatically by Xamarin Studio Community to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace TotemAppIos
{
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		MaterialControls.MDButton btnChecklist { get; set; }

		[Outlet]
		MaterialControls.MDButton btnEigenschappen { get; set; }

		[Outlet]
		MaterialControls.MDButton btnProfielen { get; set; }

		[Outlet]
		MaterialControls.MDButton btnTotems { get; set; }

		[Outlet]
		UIKit.UIImageView imgMountain { get; set; }

		[Outlet]
		UIKit.UIImageView imgTotem { get; set; }

		[Outlet]
		UIKit.UILabel lblChecklistButton { get; set; }

		[Outlet]
		UIKit.UILabel lblEigenschappenButton { get; set; }

		[Outlet]
		UIKit.UILabel lblProfielenButton { get; set; }

		[Outlet]
		UIKit.UILabel lblTitle { get; set; }

		[Outlet]
		UIKit.UILabel lblTotemsButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgMountain != null) {
				imgMountain.Dispose ();
				imgMountain = null;
			}

			if (imgTotem != null) {
				imgTotem.Dispose ();
				imgTotem = null;
			}

			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (btnTotems != null) {
				btnTotems.Dispose ();
				btnTotems = null;
			}

			if (lblTotemsButton != null) {
				lblTotemsButton.Dispose ();
				lblTotemsButton = null;
			}

			if (btnEigenschappen != null) {
				btnEigenschappen.Dispose ();
				btnEigenschappen = null;
			}

			if (btnProfielen != null) {
				btnProfielen.Dispose ();
				btnProfielen = null;
			}

			if (btnChecklist != null) {
				btnChecklist.Dispose ();
				btnChecklist = null;
			}

			if (lblEigenschappenButton != null) {
				lblEigenschappenButton.Dispose ();
				lblEigenschappenButton = null;
			}

			if (lblProfielenButton != null) {
				lblProfielenButton.Dispose ();
				lblProfielenButton = null;
			}

			if (lblChecklistButton != null) {
				lblChecklistButton.Dispose ();
				lblChecklistButton = null;
			}
		}
	}
}
