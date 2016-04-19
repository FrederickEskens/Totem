﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using ServiceStack.Text;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TotemAppCore;

namespace TotemAndroid {
    [Activity (Label = "Eigenschappen", WindowSoftInputMode = SoftInput.AdjustPan)]			
	public class EigenschappenActivity : BaseActivity {
		EigenschapAdapter eigenschapAdapter;
		ListView allEigenschappenListView;
		List<Eigenschap> eigenschappenList;

		Toast mToastShort;
		Toast mToastLong;

		RelativeLayout bottomBar;

		EditText query;
		TextView title;
		ImageButton back;
		ImageButton search;

		IMenu menu;

        ISharedPreferences sharedPrefs;

		MyOnCheckBoxClickListener mListener;

		bool fullList = true;

		protected override void OnCreate (Bundle bundle) {
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.AllEigenschappen);

			//Action bar
			InitializeActionBar (SupportActionBar);
			title = ActionBarTitle;
			query = ActionBarQuery;
			search = ActionBarSearch;
			back = ActionBarBack;

			mToastShort = Toast.MakeText (this, "", ToastLength.Short);
			mToastLong = Toast.MakeText (this, "", ToastLength.Long);

			eigenschappenList = _appController.Eigenschappen;

			//listener to pass to EigenschapAdapter containing context
			mListener = new MyOnCheckBoxClickListener (this);

			eigenschapAdapter = new EigenschapAdapter (this, _appController.Eigenschappen, mListener);
			allEigenschappenListView = FindViewById<ListView> (Resource.Id.all_eigenschappen_list);
			allEigenschappenListView.Adapter = eigenschapAdapter;

			title.Text = "Eigenschappen";
			query.Hint = "Zoek eigenschap";

			//hide keyboard when scrolling through list
			allEigenschappenListView.SetOnTouchListener(new MyOnTouchListener(this, query));

			LiveSearch ();

            sharedPrefs = GetSharedPreferences("data", FileCreationMode.Private);

			var vind = FindViewById<LinearLayout> (Resource.Id.vind);
			vind.Click += VindTotem;

			bottomBar = FindViewById<RelativeLayout> (Resource.Id.bottomBar);

			search.Visibility = ViewStates.Visible;
			search.Click += (sender, e) => ToggleSearch ();

			//hide keyboard when enter is pressed
			query.EditorAction += (sender, e) => {
				if (e.ActionId == ImeAction.Search)
					KeyboardHelper.HideKeyboard(this);
				else
					e.Handled = false;
			};
		}

		protected override void OnResume ()	{
			base.OnResume ();

			_appController.UpdateCounter += updateCounter;
			_appController.ShowSelected += ShowSelectedOnly;
			_appController.NavigationController.GotoTotemResultEvent+= StartResultTotemsActivity;

            //update eigenschappenlist from sharedprefs
            var ser = sharedPrefs.GetString("eigenschappen", "error");
            if (!ser.Equals("error")) {
                _appController.Eigenschappen = JsonSerializer.DeserializeFromString<List<Eigenschap>>(ser);
                eigenschapAdapter.UpdateData(_appController.Eigenschappen);
            }

            eigenschapAdapter.NotifyDataSetChanged ();
            HideSearch();

            //this needs a delay for some reason
            Task.Factory.StartNew(() => Thread.Sleep(50)).ContinueWith(t => {
				allEigenschappenListView.SetSelection (0);
			}, TaskScheduler.FromCurrentSynchronizationContext());

            _appController.FireUpdateEvent ();
		}

		protected override void OnPause () {
			base.OnPause ();

			_appController.UpdateCounter -= updateCounter;
			_appController.ShowSelected -= ShowSelectedOnly;
			_appController.NavigationController.GotoTotemResultEvent-= StartResultTotemsActivity;

            //save eigenschappenlist state in sharedprefs
            var editor = sharedPrefs.Edit();
            var ser = ServiceStack.Text.JsonSerializer.SerializeToString(_appController.Eigenschappen);
            editor.PutString("eigenschappen", ser);
            editor.Commit();
		}

		void updateCounter () {
			int count = _appController.Eigenschappen.FindAll (x => x.selected).Count;
			var tvNumberSelected = FindViewById<TextView> (Resource.Id.selected);
			tvNumberSelected.Text = count + " geselecteerd";
			if (count > 0)
				bottomBar.Visibility = ViewStates.Visible;
			else
				bottomBar.Visibility = ViewStates.Gone;
		}

		//toggles the search bar
		void ToggleSearch() {
			if (query.Visibility == ViewStates.Visible) {
				HideSearch();
			} else {
				back.Visibility = ViewStates.Gone;
				title.Visibility = ViewStates.Gone;
				query.Visibility = ViewStates.Visible;
				KeyboardHelper.ShowKeyboard (this, query);
				query.Text = "";
				query.RequestFocus ();
				search.SetImageResource (Resource.Drawable.ic_close_white_24dp);
			}
		}

		//hides the search bar
		void HideSearch() {
			back.Visibility = ViewStates.Visible;
			title.Visibility = ViewStates.Visible;
			query.Visibility = ViewStates.Gone;
            search.SetImageResource(Resource.Drawable.ic_search_white_24dp);
            KeyboardHelper.HideKeyboard (this);
			eigenschapAdapter.UpdateData (_appController.Eigenschappen);
			eigenschapAdapter.NotifyDataSetChanged ();
			query.Text = "";
			fullList = true;
            if(menu != null)
			    UpdateOptionsMenu ();
		}

		//update list after every keystroke
		void LiveSearch() {
			query.AfterTextChanged += (sender, args) => {
				Search();
				if(query.Text.Equals(""))
					fullList = true;
			};
		}

		//shows only totems that match the query
		void Search() {
			fullList = false;
			eigenschappenList = _appController.FindEigenschapOpNaam (query.Text);
			eigenschapAdapter.UpdateData (eigenschappenList);
			eigenschapAdapter.NotifyDataSetChanged ();
			if(query.Length() > 0)
				allEigenschappenListView.SetSelection (0);
		}

		//renders list of totems with frequencies based on selected eigenschappen
		//and redirects to TotemsActivity to view them
		void VindTotem(object sender, EventArgs e) {
			_appController.CalculateResultlist(_appController.Eigenschappen);
		}

		void StartResultTotemsActivity() {
			var totemsActivity = new Intent (this, typeof(ResultTotemsActivity));
			StartActivity (totemsActivity);
		}

		//create options menu
		public override bool OnCreateOptionsMenu(IMenu m) {
			menu = m;
			MenuInflater.Inflate(Resource.Menu.EigenschapSelectieMenu, menu);
			IMenuItem item = menu.FindItem (Resource.Id.full);
			item.SetVisible (false);
			return base.OnCreateOptionsMenu(menu);
		}

		//options menu: add profile, view selection of view full list
		public override bool OnOptionsItemSelected(IMenuItem item) {
			switch (item.ItemId) {

			//reset selection
			case Resource.Id.reset:
                query.Text = "";
				fullList = true;
				foreach (Eigenschap e in eigenschappenList)
					e.selected = false;
				eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				eigenschapAdapter.NotifyDataSetChanged ();
                UpdateOptionsMenu ();
                
                //this needs a delay for some reason
                Task.Factory.StartNew(() => Thread.Sleep(50)).ContinueWith(t => {
                    allEigenschappenListView.SetSelection(0);
                }, TaskScheduler.FromCurrentSynchronizationContext());

                return true;
			
			//show selected only
			case Resource.Id.select:
				ShowSelectedOnly ();
				return true;

			//show full list
			case Resource.Id.full:
				query.Text = "";
				fullList = true;
				UpdateOptionsMenu ();
				eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				eigenschapAdapter.NotifyDataSetChanged ();
				return true;

			//show full list
			case Resource.Id.tinderView:
				var totemsActivity = new Intent (this, typeof(TinderEigenschappenActivity));
				StartActivity (totemsActivity);
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		void ShowSelectedOnly() {
			List<Eigenschap> list = GetSelectedEigenschappen ();
			if (list.Count == 0) {
				mToastShort.SetText ("Geen eigenschappen geselecteerd");
				mToastShort.Show ();
			} else {
				fullList = false;
				UpdateOptionsMenu ();
				eigenschapAdapter.UpdateData (list);
				eigenschapAdapter.NotifyDataSetChanged ();
				bottomBar.Visibility = ViewStates.Visible;
			}
		}

		//changes the options menu items according to list
		//delay of 0.5 seconds to take animation into account
		void UpdateOptionsMenu() {
			IMenuItem s = menu.FindItem (Resource.Id.select);
			IMenuItem f = menu.FindItem (Resource.Id.full);

			Task.Factory.StartNew(() => Thread.Sleep(500)).ContinueWith(t => {
				if (fullList) {
					s.SetVisible (true);
					f.SetVisible (false);
				} else {
					s.SetVisible (false);
					f.SetVisible (true);
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		//returns list of eigenschappen that have been checked
		List<Eigenschap> GetSelectedEigenschappen() {
			var result = new List<Eigenschap> ();
			foreach(Eigenschap e in _appController.Eigenschappen)
				if (e.selected)
					result.Add (e);

			return result;
		}

		//return to full list and empty search field when 'back' is pressed
		//this happens only when a search query is currently entered
		public override void OnBackPressed() {
			if (query.Visibility == ViewStates.Visible) {
				HideSearch ();
			} else if (!fullList) {
				query.Text = "";
				fullList = true;
				UpdateOptionsMenu ();
				eigenschapAdapter.UpdateData (_appController.Eigenschappen);
				eigenschapAdapter.NotifyDataSetChanged ();
			} else {
				base.OnBackPressed ();
			}
		}
	}
}