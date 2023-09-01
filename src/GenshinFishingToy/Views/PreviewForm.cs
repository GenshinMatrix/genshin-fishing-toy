using GenshinFishingToy.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Resources;

namespace GenshinFishingToy.Views;

public partial class Preview : Form
{
    public Preview()
    {
        InitializeComponent();

        using Stream stream = ResourceUtils.GetStream("pack://application:,,,/GenshinFishingToy;component/Resources/favicon.ico");
        Icon = new Icon(stream);
    }

    public void SetImage(Bitmap bitmap)
    {
        try
        {
            pictureBox1.Image = bitmap?.Clone() as Bitmap;
        }
        catch
        {
        }
    }
}
