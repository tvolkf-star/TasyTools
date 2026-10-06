using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

class App : Form {
 string src, ff; double sx=.25, sy=.65, fx=.75, fy=.35; int pickStage=0;
 PictureBox pic=new PictureBox(); ComboBox move=new ComboBox(), dur=new ComboBox(), size=new ComboBox();
 TextBox output=new TextBox(); Label pt=new Label(), status=new Label(); Button go=new Button();

 [STAThread] static void Main(){Application.EnableVisualStyles();Application.Run(new App());}
 App(){
  Text="TASY Still Motion"; ClientSize=new Size(820,700); Font=new Font("Segoe UI",10); BackColor=Color.White; AllowDrop=true; ff=FindFF();
  Controls.Add(new Label{Text="TASY Still Motion",Font=new Font("Segoe UI Semibold",22),AutoSize=true,Location=new Point(24,18)});
  Controls.Add(new Label{Text="Большой рендер → движение камеры. Ткните START, затем FINISH.",AutoSize=true,Location=new Point(27,65)});
  pic.SetBounds(28,105,764,360); pic.BorderStyle=BorderStyle.FixedSingle; pic.SizeMode=PictureBoxSizeMode.Zoom; pic.BackColor=Color.FromArgb(245,245,245); pic.Cursor=Cursors.Cross; pic.AllowDrop=true; Controls.Add(pic);
  Button add=new Button{Text="Добавить картинку",Location=new Point(28,480),Size=new Size(155,34)}; Controls.Add(add);
  pt.Text="START: 25% · 65%   |   FINISH: 75% · 35%";pt.AutoSize=true;pt.Location=new Point(200,489);Controls.Add(pt);
  MakeLabel("Движение",28,540); Setup(move,new[]{"LOOK IN","LOOK UP","LOOK SIDE","DETAIL","DRIFT","LOOP"},105,535,145);
  MakeLabel("Время",270,540); Setup(dur,new[]{"5 sec","10 sec","15 sec","20 sec"},320,535,85);
  MakeLabel("Размер",425,540); Setup(size,new[]{"480 FORUM","1200 WEB","SOURCE"},485,535,160);
  MakeLabel("Сохранить:",28,588); output.SetBounds(105,583,565,28); Controls.Add(output);
  Button browse=new Button{Text="Выбрать",Location=new Point(680,581),Size=new Size(112,32)};Controls.Add(browse);
  go.Text="ОЖИВИТЬ";go.Font=new Font("Segoe UI Semibold",11);go.SetBounds(28,635,190,40);Controls.Add(go);
  status.Text=ff!=null?"Готово к работе.":"FFmpeg не найден.";status.SetBounds(235,645,350,25);Controls.Add(status);
  LinkLabel link=new LinkLabel{Text="TASY.PRO ★★★★★",AutoSize=true,Location=new Point(655,645),LinkColor=Color.Black};Controls.Add(link);
  add.Click+=delegate{using(OpenFileDialog d=new OpenFileDialog()){d.Filter="Изображения|*.jpg;*.jpeg;*.png;*.webp;*.bmp";if(d.ShowDialog()==DialogResult.OK)Load(d.FileName);}};
  browse.Click+=delegate{using(FolderBrowserDialog d=new FolderBrowserDialog()){if(d.ShowDialog()==DialogResult.OK)output.Text=d.SelectedPath;}};
  pic.MouseClick+=Pick; go.Click+=Render; link.LinkClicked+=delegate{Process.Start(new ProcessStartInfo("https://tasy.pro/"){UseShellExecute=true});};
  DragEnter+=DE;DragDrop+=DD;pic.DragEnter+=DE;pic.DragDrop+=DD;
 }
 void MakeLabel(string s,int x,int y){Controls.Add(new Label{Text=s,AutoSize=true,Location=new Point(x,y)});}
 void Setup(ComboBox c,string[] a,int x,int y,int w){c.Items.AddRange(a);c.SelectedIndex=0;c.DropDownStyle=ComboBoxStyle.DropDownList;c.SetBounds(x,y,w,28);Controls.Add(c);}
 void DE(object s,DragEventArgs e){if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;}
 void DD(object s,DragEventArgs e){string[] a=(string[])e.Data.GetData(DataFormats.FileDrop);if(a.Length>0)Load(a[0]);}
 void Load(string p){try{using(Image i=Image.FromFile(p))pic.Image=new Bitmap(i);src=p;sx=.25;sy=.65;fx=.75;fy=.35;pickStage=0;pt.Text="START: 25% · 65%   |   FINISH: 75% · 35%";if(output.Text=="")output.Text=Path.GetDirectoryName(p);status.Text="Картинка загружена. Выберите START, затем FINISH.";}catch{MessageBox.Show("Не удалось открыть изображение.");}}
 Rectangle IR(){double ir=pic.Image.Width/(double)pic.Image.Height,br=pic.Width/(double)pic.Height;if(ir>br){int h=(int)(pic.Width/ir);return new Rectangle(0,(pic.Height-h)/2,pic.Width,h);}int w=(int)(pic.Height*ir);return new Rectangle((pic.Width-w)/2,0,w,pic.Height);}
 void Pick(object sender,MouseEventArgs e){if(pic.Image==null)return;Rectangle r=IR();if(!r.Contains(e.Location))return;double x=(e.X-r.Left)/(double)r.Width,y=(e.Y-r.Top)/(double)r.Height;if(pickStage==0){sx=x;sy=y;pickStage=1;status.Text="START выбран. Теперь FINISH.";}else{fx=x;fy=y;pickStage=0;status.Text="FINISH выбран. Можно запускать.";}pt.Text=String.Format("START: {0:0}% · {1:0}%   |   FINISH: {2:0}% · {3:0}%",sx*100,sy*100,fx*100,fy*100);}
 string FindFF(){string b=AppDomain.CurrentDomain.BaseDirectory;string[] a={Path.Combine(b,"tools","ffmpeg.exe"),Path.Combine(b,"ffmpeg.exe")};foreach(string p in a)if(File.Exists(p))return p;try{ProcessStartInfo q=new ProcessStartInfo("where.exe","ffmpeg.exe"){UseShellExecute=false,RedirectStandardOutput=true,CreateNoWindow=true};using(Process p=Process.Start(q)){string l=p.StandardOutput.ReadLine();p.WaitForExit();if(!String.IsNullOrWhiteSpace(l)&&File.Exists(l))return l;}}catch{}return null;}
 string F(double n){return n.ToString("0.######",CultureInfo.InvariantCulture);}
 void Render(object sender,EventArgs e){
  ff=FindFF(); if(ff==null){MessageBox.Show("FFmpeg не найден: tools\\ffmpeg.exe, рядом с EXE или PATH.");return;}
  if(src==null){MessageBox.Show("Добавьте картинку.");return;}
  string dest=output.Text.Trim();if(dest==""){MessageBox.Show("Выберите папку.");return;}Directory.CreateDirectory(dest);

  int[] dd={5,10,15,20}; int sec=dd[dur.SelectedIndex]; string m=move.Text;
  int limit=size.SelectedIndex==0?480:(size.SelectedIndex==1?1200:0);
  int srcW,srcH,ow,oh; using(Image im=Image.FromFile(src)){srcW=im.Width;srcH=im.Height;}
  if(limit==0){ow=srcW;oh=srcH;}
  else if(srcW>=srcH){ow=limit;oh=(int)Math.Round(limit*srcH/(double)srcW);}
  else{oh=limit;ow=(int)Math.Round(limit*srcW/(double)srcH);}
  ow=Math.Max(2,ow/2*2); oh=Math.Max(2,oh/2*2);

  // Real camera window: create a larger working image and move a fixed crop across it.
  // No zoompan: it was the source of the false vertical "fall".
  double territory=m=="DETAIL"?1.75:(m=="DRIFT"?1.32:1.55);
  int iw=(int)Math.Ceiling(ow*territory), ih=(int)Math.Ceiling(oh*territory);
  iw=(iw+1)/2*2; ih=(ih+1)/2*2;

  // Keep the 4x anti-jitter principle from the successful v4/v5 engine.
  int k=4, OW=ow*k, OH=oh*k, IW=iw*k, IH=ih*k;
  int ceiling=7200;
  if(IW>ceiling||IH>ceiling){
    double c=Math.Min(ceiling/(double)IW,ceiling/(double)IH);
    IW=Math.Max(OW,((int)(IW*c))/2*2); IH=Math.Max(OH,((int)(IH*c))/2*2);
  }

  string u="n/(30*"+sec+".0-1)";
  string ease="("+u+"*"+u+"*(3-2*"+u+"))";
  string q=m=="LOOP" ? "(0.5-0.5*cos(2*PI*"+u+"))" : ease;

  // START/FINISH are centres of the camera window in normalized image coordinates.
  string x0=F(sx)+"*(iw-"+OW+")";
  string x1=F(fx)+"*(iw-"+OW+")";
  string y0=F(sy)+"*(ih-"+OH+")";
  string y1=F(fy)+"*(ih-"+OH+")";
  string cx="("+x0+"+("+x1+"-"+x0+")*"+q+")";
  string cy="("+y0+"+("+y1+"-"+y0+")*"+q+")";

  // Presets may bend the route slightly, but never replace the chosen trajectory.
  if(m=="LOOK UP") cy="("+cy+"-0.08*"+OH+"*"+q+")";
  if(m=="LOOK SIDE") cx="("+cx+"+0.08*"+OW+"*"+q+")";

  string vf=
    "scale="+IW+":"+IH+":force_original_aspect_ratio=increase:flags=lanczos,"+
    "crop="+IW+":"+IH+","+
    "crop="+OW+":"+OH+":x='max(0,min(iw-"+OW+","+cx+"))':y='max(0,min(ih-"+OH+","+cy+"))',"+
    "scale="+ow+":"+oh+":flags=lanczos,"+
    // shadow belongs to the edge; frame is exactly on the edge, not inset.
    "vignette=PI/7,"+
    "drawbox=x=0:y=0:w=iw:h=ih:color=white@0.78:t=6,"+
    "setsar=1,fps=30";

  string dst=Path.Combine(dest,Path.GetFileNameWithoutExtension(src)+"_"+m.ToLower().Replace(" ","_")+".mp4");
  string args="-hide_banner -loglevel error -y -loop 1 -framerate 30 -i \""+src+"\" -t "+sec+
    " -vf \""+vf+"\" -an -map_metadata -1 -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -movflags +faststart \""+dst+"\"";

  go.Enabled=false;status.Text="Камера пошла...";Application.DoEvents();
  try{
    ProcessStartInfo psi=new ProcessStartInfo(ff,args){UseShellExecute=false,RedirectStandardError=true,CreateNoWindow=true};
    string err="";using(Process pr=Process.Start(psi)){err=pr.StandardError.ReadToEnd();pr.WaitForExit();if(pr.ExitCode!=0)throw new Exception(err);}
    status.Text=String.Format("Готово · {0:0.0} МБ",new FileInfo(dst).Length/1048576.0);
  }catch(Exception ex){
    status.Text="Ошибка FFmpeg.";string msg=ex.Message;if(msg.Length>1400)msg=msg.Substring(msg.Length-1400);
    MessageBox.Show("Не удалось собрать анимацию.\\r\\n\\r\\n"+msg);
  }
  go.Enabled=true;
 }
}