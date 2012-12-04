using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Plugins.MovingPictures;
using System.Threading;
using Cornerstone.GUI.Dialogs;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures.ConfigScreen.Popups;
using Cornerstone.Database.Tables;

namespace MPExtended.PlugIns.Scrapers.MovingPictures
{
    public partial class MovingPicturesConfig : Form
    {

        public MovingPicturesConfig()
        {
            InitializeComponent();

            MovingPicturesSettings settings = MovingPicturesCore.Settings;

            foreach (DBSetting s in settings.AllSettings)
            {
                if (s.Grouping != null && s.Grouping.Count > 0)
                {
                    //hide top-level entries
                    if (s.Grouping[0].Equals("MediaPortal GUI")
                         || s.Grouping[0].Equals("follw.it"))
                    {
                        s.Hidden = true;
                    }

                    //hide entries by key
                    if(s.Key.Equals("enable_debug_allskinproperties"))
                    {
                        s.Hidden = true;
                    }
                }
            }
        }

        private void MovingPicturesConfig_Load(object sender, EventArgs e)
        {

        }
    }
}
