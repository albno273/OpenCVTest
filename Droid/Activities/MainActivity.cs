using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.Design.Widget;
using OpenCV.ImgCodecs;
using OpenCV.ImgProc;
using OpenCV.Android;
using OpenCV.Features2d;
using OpenCV.Utils;
using OpenCV.Core;

namespace OpenCVTest.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon",
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity
    {
        protected override int LayoutResource => Resource.Layout.activity_main;

        ViewPager pager;
        TabsAdapter adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            adapter = new TabsAdapter(this, SupportFragmentManager);
            pager = FindViewById<ViewPager>(Resource.Id.viewpager);
            var tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            pager.Adapter = adapter;
            tabs.SetupWithViewPager(pager);
            pager.OffscreenPageLimit = 3;

            pager.PageSelected += (sender, args) =>
            {
                var fragment = adapter.InstantiateItem(pager, args.Position) as IFragmentVisible;

                fragment?.BecameVisible();
            };

            Toolbar.MenuItemClick += (sender, e) =>
            {
                var intent = new Intent(this, typeof(AddItemActivity)); ;
                StartActivity(intent);
            };

            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(false);

            var src = new Mat[2];
            var dst = new Mat[2];
            var keyPoints1 = new MatOfKeyPoint();
            var keyPoints2 = new MatOfKeyPoint();
            var descripter1 = new Mat();
            var descripter2 = new Mat();
            var dmatch = new MatOfDMatch();
            var output = new Mat();

            src[0] = Imgcodecs.Imread("path/to/source/1.png");
            src[1] = Imgcodecs.Imread("path/to/source/2.png");
            dst[0] = new Mat();
            dst[1] = new Mat();
            Imgproc.CvtColor(src[0], dst[0], Imgproc.COLORBayerGR2GRAY);
            Imgproc.CvtColor(src[1], dst[1], Imgproc.COLORBayerGR2GRAY);

            var akaze = FeatureDetector.Create(FeatureDetector.Akaze);
            var executor = DescriptorExtractor.Create(DescriptorExtractor.Akaze);

            akaze.Detect(dst[0], keyPoints1);
            akaze.Detect(dst[1], keyPoints2);

            executor.Compute(dst[0], keyPoints1, descripter1);
            executor.Compute(dst[1], keyPoints2, descripter2);

            var matcher = DescriptorMatcher.Create(DescriptorMatcher.BruteforceHamming);
            matcher.Match(descripter1, descripter2, dmatch);

            Features2d.DrawMatches(src[0], keyPoints1, src[1], keyPoints2, dmatch, output);

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }
    }

    class TabsAdapter : FragmentStatePagerAdapter
    {
        string[] titles;

        public override int Count => titles.Length;

        public TabsAdapter(Context context, Android.Support.V4.App.FragmentManager fm) : base(fm)
        {
            titles = context.Resources.GetTextArray(Resource.Array.sections);
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position) =>
                            new Java.Lang.String(titles[position]);

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0: return BrowseFragment.NewInstance();
                case 1: return AboutFragment.NewInstance();
            }
            return null;
        }

        public override int GetItemPosition(Java.Lang.Object frag) => PositionNone;
    }
}
