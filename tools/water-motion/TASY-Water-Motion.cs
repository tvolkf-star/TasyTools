using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TasyWaterMotion
{
    public sealed class MainForm : Form
    {
        Bitmap source, mask, preview;
        PictureBox canvas = new PictureBox();
        TrackBar strength = new TrackBar();
        ComboBox preset = new ComboBox();
        NumericUpDown seconds = new NumericUpDown();
        NumericUpDown brushSize = new NumericUpDown();
        Timer timer = new Timer();
        Button open = new Button(), clear = new Button(), play = new Button(), export = new Button();
        Label hint = new Label();
        bool painting, erasing;
        Point last;
        DateTime started;
        

        public MainForm()
        {
            Text = "TASY Water Motion · prototype";
            Width = 1100; Height = 820; StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30,30,30); ForeColor = Color.White;

            var bar = new FlowLayoutPanel { Dock=DockStyle.Top, Height=62, Padding=new Padding(8), AutoSize=false, WrapContents=false, AutoScroll=true };
            open.Text="OPEN IMAGE"; clear.Text="CLEAR MASK"; play.Text="PREVIEW"; export.Text="EXPORT MP4";
            foreach(var b in new[]{open,clear,play,export}) { b.AutoSize=true; b.Height=34; bar.Controls.Add(b); }

            preset.DropDownStyle=ComboBoxStyle.DropDownList; preset.Width=120;
            preset.Items.AddRange(new object[]{"CALM","RIPPLE","WIND"}); preset.SelectedIndex=0; bar.Controls.Add(preset);
            strength.Minimum=1; strength.Maximum=18; strength.Value=5; strength.Width=130; bar.Controls.Add(strength);
            seconds.Minimum=2; seconds.Maximum=20; seconds.Value=5; seconds.Width=55; bar.Controls.Add(seconds);
            brushSize.Minimum=10; brushSize.Maximum=500; brushSize.Value=120; brushSize.Increment=10; brushSize.Width=58; bar.Controls.Add(brushSize);
            hint.Text=" BRUSH px   PAINT: LEFT   ERASE: RIGHT"; hint.AutoSize=true; hint.Padding=new Padding(0,9,0,0); bar.Controls.Add(hint);

            canvas.Dock=DockStyle.Fill; canvas.BackColor=Color.FromArgb(18,18,18);
            canvas.SizeMode=PictureBoxSizeMode.Zoom; canvas.Cursor=Cursors.Cross;
            AllowDrop=true; canvas.AllowDrop=true;
            Controls.Add(canvas); Controls.Add(bar);

            open.Click += (s,e)=>OpenImage();
            clear.Click += (s,e)=>{ if(mask!=null){ using(var g=Graphics.FromImage(mask)) g.Clear(Color.Black); DrawFrame(0,true); } };
            play.Click += (s,e)=>TogglePreview();
            export.Click += (s,e)=>ExportMp4();
            canvas.MouseDown += CanvasDown; canvas.MouseMove += CanvasMove; canvas.MouseUp += (s,e)=>painting=false;
            DragEnter += FileDragEnter; DragDrop += FileDragDrop;
            canvas.DragEnter += FileDragEnter; canvas.DragDrop += FileDragDrop;
            timer.Interval=33; timer.Tick += (s,e)=> {
                double d=(double)seconds.Value;
                double t=((DateTime.Now-started).TotalSeconds%d)/d;
                DrawFrame(t,false);
            };
        }

        void OpenImage()
        {
            using(var d=new OpenFileDialog { Filter="Images|*.png;*.jpg;*.jpeg;*.bmp;*.webp" })
            {
                if(d.ShowDialog()==DialogResult.OK) LoadImage(d.FileName);
            }
        }

        bool IsImageFile(string path)
        {
            string ext=Path.GetExtension(path).ToLowerInvariant();
            return ext==".png" || ext==".jpg" || ext==".jpeg" || ext==".bmp" || ext==".webp";
        }

        void LoadImage(string path)
        {
            try
            {
                if(!IsImageFile(path)) throw new Exception("Unsupported image format.");
                if(preview != null) { preview.Dispose(); preview=null; }
                if(source != null) { source.Dispose(); source=null; }
                if(mask != null) { mask.Dispose(); mask=null; }
                using(var tmp=new Bitmap(path)) source=new Bitmap(tmp);
                mask=new Bitmap(source.Width,source.Height,PixelFormat.Format24bppRgb);
                using(var g=Graphics.FromImage(mask)) g.Clear(Color.Black);
                DrawFrame(0,true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Open image");
            }
        }

        void FileDragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files=(string[])e.Data.GetData(DataFormats.FileDrop);
                if(files.Length>0 && IsImageFile(files[0])) e.Effect=DragDropEffects.Copy;
                else e.Effect=DragDropEffects.None;
            }
            else e.Effect=DragDropEffects.None;
        }

        void FileDragDrop(object sender, DragEventArgs e)
        {
            string[] files=(string[])e.Data.GetData(DataFormats.FileDrop);
            if(files!=null && files.Length>0) LoadImage(files[0]);
        }

        Rectangle ImageRect()
        {
            if(source==null) return Rectangle.Empty;
            double k=Math.Min((double)canvas.ClientSize.Width/source.Width,(double)canvas.ClientSize.Height/source.Height);
            int w=(int)(source.Width*k), h=(int)(source.Height*k);
            return new Rectangle((canvas.ClientSize.Width-w)/2,(canvas.ClientSize.Height-h)/2,w,h);
        }

        Point ToImage(Point p)
        {
            var r=ImageRect();
            if(r.Width<1) return Point.Empty;
            int x=(int)((p.X-r.X)*(double)source.Width/r.Width);
            int y=(int)((p.Y-r.Y)*(double)source.Height/r.Height);
            return new Point(Math.Max(0,Math.Min(source.Width-1,x)),Math.Max(0,Math.Min(source.Height-1,y)));
        }

        void CanvasDown(object s, MouseEventArgs e)
        {
            if(source==null || !ImageRect().Contains(e.Location)) return;
            painting=true; erasing=e.Button==MouseButtons.Right; last=ToImage(e.Location); PaintMask(last,last);
        }
        void CanvasMove(object s, MouseEventArgs e)
        {
            if(!painting || source==null) return;
            var p=ToImage(e.Location); PaintMask(last,p); last=p;
        }
        void PaintMask(Point a, Point b)
        {
            using(var g=Graphics.FromImage(mask))
            int brush=(int)brushSize.Value;
            using(var pen=new Pen(erasing?Color.Black:Color.White,brush){StartCap=System.Drawing.Drawing2D.LineCap.Round,EndCap=System.Drawing.Drawing2D.LineCap.Round})
            {
                g.DrawLine(pen,a,b);
                using(var brush=new SolidBrush(erasing?Color.Black:Color.White))
                    g.FillEllipse(brush,b.X-brush/2,b.Y-brush/2,brush,brush);
            }
            DrawFrame(0,true);
        }

        void TogglePreview()
        {
            if(source==null) return;
            if(timer.Enabled){timer.Stop(); play.Text="PREVIEW"; DrawFrame(0,true);}
            else {started=DateTime.Now; timer.Start(); play.Text="STOP";}
        }

        double Wave(int y,double t)
        {
            double s=strength.Value;
            string p=(string)preset.SelectedItem;
            double scale=p=="CALM"?0.55:p=="WIND"?1.55:1.0;
            double a=Math.Sin(2*Math.PI*t + y*0.085);
            double b=Math.Sin(4*Math.PI*t - y*0.031 + 1.2);
            double c=Math.Sin(6*Math.PI*t + y*0.014 + 2.4);
            return s*scale*(0.58*a+0.28*b+0.14*c);
        }

        Bitmap Render(double t)
        {
            var dst=new Bitmap(source.Width,source.Height,PixelFormat.Format24bppRgb);
            using(var src24=new Bitmap(source.Width,source.Height,PixelFormat.Format24bppRgb))
            {
                using(var g=Graphics.FromImage(src24)) g.DrawImageUnscaled(source,0,0);
                var rect=new Rectangle(0,0,source.Width,source.Height);
                var sd=src24.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
                var md=mask.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
                var dd=dst.LockBits(rect,ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
                unsafe
                {
                    for(int y=0;y<source.Height;y++)
                    {
                        byte* sp=(byte*)sd.Scan0+y*sd.Stride;
                        byte* mp=(byte*)md.Scan0+y*md.Stride;
                        byte* dp=(byte*)dd.Scan0+y*dd.Stride;
                        double row=Wave(y,t);
                        for(int x=0;x<source.Width;x++)
                        {
                            double m=mp[x*3]/255.0;
                            if(m<0.002){ dp[x*3]=sp[x*3]; dp[x*3+1]=sp[x*3+1]; dp[x*3+2]=sp[x*3+2]; continue; }
                            double perspective=0.25 + 1.35*((double)y/Math.Max(1,source.Height-1));
                            double local=(row + strength.Value*0.22*Math.Sin(2*Math.PI*t + x*0.024 + y*0.018))*perspective;
                            double vertical=strength.Value*0.38*perspective*Math.Sin(2*Math.PI*t + x*0.017 - y*0.011 + 0.7);
                            int sx=(int)Math.Round(x+local*m);
                            int sy=(int)Math.Round(y+vertical*m);
                            sx=Math.Max(0,Math.Min(source.Width-1,sx));
                            sy=Math.Max(0,Math.Min(source.Height-1,sy));
                            byte* sample=(byte*)sd.Scan0+sy*sd.Stride;
                            dp[x*3]=sample[sx*3]; dp[x*3+1]=sample[sx*3+1]; dp[x*3+2]=sample[sx*3+2];
                        }
                    }
                }
                src24.UnlockBits(sd); mask.UnlockBits(md); dst.UnlockBits(dd);
            }
            return dst;
        }

        void DrawFrame(double t,bool showMask)
        {
            if(preview != null) preview.Dispose(); preview=Render(t);
            if(showMask)
            {
                using(var g=Graphics.FromImage(preview))
                using(var overlay=new SolidBrush(Color.FromArgb(80,0,180,255)))
                {
                    for(int y=0;y<mask.Height;y+=4)
                    for(int x=0;x<mask.Width;x+=4)
                        if(mask.GetPixel(x,y).R>20) g.FillRectangle(overlay,x,y,4,4);
                }
            }
            canvas.Image=preview; canvas.Invalidate();
        }

        string FindFFmpeg()
        {
            string local=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ffmpeg.exe");
            string tools=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tools","ffmpeg.exe");
            if(File.Exists(tools)) return tools; if(File.Exists(local)) return local;
            return "ffmpeg";
        }

        void ExportMp4()
        {
            if(source==null) return;
            timer.Stop(); play.Text="PREVIEW";
            string outputFile;
            using(var d=new SaveFileDialog { Filter="MP4 video|*.mp4", FileName="water-motion.mp4" })
            {
                if(d.ShowDialog()!=DialogResult.OK) return;
                outputFile=d.FileName;
            }
            string temp=Path.Combine(Path.GetTempPath(),"TasyWater_"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            try
            {
                int fps=30, frames=(int)seconds.Value*fps;
                for(int i=0;i<frames;i++)
                {
                    // i/frames intentionally excludes t=1: playback wraps to frame 0 without a duplicate pause frame.
                    using(var f=Render((double)i/frames))
                        f.Save(Path.Combine(temp, "frame_" + i.ToString("00000") + ".png"), ImageFormat.Png);
                    Text = "TASY Water Motion · rendering " + (i + 1) + "/" + frames;
                    Application.DoEvents();
                }
                string args = "-y -framerate " + fps + " -i \"" + Path.Combine(temp, "frame_%05d.png") + "\" -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -movflags +faststart \"" + outputFile + "\"";
                var psi=new ProcessStartInfo(FindFFmpeg(),args){UseShellExecute=false,CreateNoWindow=true};
                using(var p=Process.Start(psi)){p.WaitForExit(); if(p.ExitCode!=0) throw new Exception("FFmpeg returned "+p.ExitCode);}
                MessageBox.Show("Done:\n"+outputFile,"TASY Water Motion");
            }
            catch(Exception ex){MessageBox.Show(ex.Message,"Export error");}
            finally
            {
                Text="TASY Water Motion · prototype";
                try{Directory.Delete(temp,true);}catch{}
                DrawFrame(0,true);
            }
        }

        [STAThread]
        static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new MainForm());}
    }
}
