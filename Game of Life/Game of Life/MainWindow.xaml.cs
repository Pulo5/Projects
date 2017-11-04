using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Game_of_Life
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private int ctvercu_sirka;
        private int ctvercu_hloubka;
        private RenderTargetBitmap buffer;
        private DrawingVisual drawingVisual = new DrawingVisual();
        private int[,,] matice;
        private bool konfigurace = false;
        private int kol = 0;
        private int rychlost;
        private bool bezi = false;
        private int userVal;
        private string color_first = "#008000";
        private string color_second = "#008000";
        private string color_third = "#008000";
        private string color_fourth = "#008000";
        private bool first_rule;
        private bool second_rule;
        private bool third_rule;
        private bool fourth_rule;

        public MainWindow()
        {
            InitializeComponent();
            ctvercu_sirka = (int)Background.Width / 12;
            ctvercu_hloubka = (int)Background.Height / 12;
            matice = new int[ctvercu_hloubka+1 , ctvercu_sirka+1, 4];
            sirka_Label.Content = "Počet bodů na šířku: " + ctvercu_sirka;
            vyska_Label.Content = "Počet bodů na výšku: " + ctvercu_hloubka;
            kol_Label.Content = "Počet kol: " + kol;
            ClrPcker_Background.SelectedColor = (Color)ColorConverter.ConvertFromString("Green"); ;
            ClrPcker_Select.SelectedColor = (Color)ColorConverter.ConvertFromString("Green"); ;
            ClrPcker_Select2.SelectedColor = (Color)ColorConverter.ConvertFromString("Gray"); ;
            ClrPcker_Select3.SelectedColor = (Color)ColorConverter.ConvertFromString("Gray"); ;
            first_ruleCeck.IsChecked = true;
            second_ruleCeck.IsChecked = true;
            third_ruleCeck.IsChecked = true;
            fourth_ruleCeck.IsChecked = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (konfigurace == true)
                return;

            base.OnRender(drawingContext);
            buffer = new RenderTargetBitmap((int)Background.Width, (int)Background.Height, 96, 96, PixelFormats.Pbgra32);
            Background.Source = buffer;
            DrawStuff();            
        }

        /// <summary>
        /// Metoda vykreslující plochu.
        /// </summary>
        private void DrawStuff()
        {
            if (buffer == null)
                return;

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                int matice_sloupec = 0;
                int matice_radek = 0;

                for (int i = 0; i <= (int)Background.Width; i += 12)
                {
                    for (int j = 0; j <= (int)Background.Height; j += 12)
                    {

                        drawingContext.DrawRectangle(new SolidColorBrush(Colors.Gray), null, new Rect(i, j, 10, 10));

                        matice[matice_radek, matice_sloupec, 0] = i - 1;
                        matice[matice_radek, matice_sloupec, 1] = j - 1;
                        matice[matice_radek, matice_sloupec, 2] = 1;
                        matice[matice_radek, matice_sloupec, 3] = 0;

                        matice_radek++;

                    }
                    matice_sloupec++;
                    matice_radek = 0;
                }
            }
            buffer.Render(drawingVisual);
        }

        /// <summary>
        /// Metoda generující náhodné prvky matice.
        /// </summary>
        /// <param name="random_number"></param>
        protected void SelectRandom(int random_number)
        {
            Random random = new Random();
            int rand_radek;
            int rand_sloupec;

            for (int i = 1; i <= random_number; i++)
            {

                if (buffer == null)
                    return;

                rand_radek = random.Next(0, ctvercu_sirka);
                rand_sloupec = random.Next(0, ctvercu_hloubka);
                matice[rand_sloupec, rand_radek, 2] = 0;        

                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {                 
                        drawingContext.DrawRectangle(new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(color_first)), null, new Rect(rand_radek * 12, rand_sloupec * 12, 10, 10));
                  
                }

                buffer.Render(drawingVisual);                
            }
        }


        /// <summary>
        /// Událost nastavující počáteční počet živých buněk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PocetBoduText_KeyDown(object sender, KeyEventArgs e)
        {           
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                    
                    if (int.TryParse(pocetBoduText.Text, out userVal))
                    {
                         if (bezi == false)
                         {
                         SelectRandom(userVal);
                         konfigurace = true;
                         }                  
                    }
            }
        }

        /// <summary>
        /// Metoda nastavuje pocet sousedu.
        /// </summary>
        private void Check()
        {
            for (int i = 0; i <= ctvercu_sirka; i++)
            {
                for (int j = 0; j <= ctvercu_hloubka; j++)
                {
                    if (matice[j, i, 2] == 0)
                    {
                        for (int x = i - 1; x <= i + 1; x++)
                        {
                            for (int y = j - 1; y <= j + 1; y++)
                            {
                                if ((x != i) || (y != j))
                                {
                                    try
                                    {                                      
                                      matice[y, x, 3] += 1;                                       
                                    }
                                    catch (System.IndexOutOfRangeException) {}

                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Metoda kontroluje a volá metodu pro další kolo.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Kolo(object sender, EventArgs e)
        {
            kol_Label.Content = "Počet kol: " + ++kol; 
            Check();
            Rules();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, rychlost);
        }

        private void Rules()
        {

            for (int i = 0; i <= ctvercu_sirka; i++)
            {
                for (int j = 0; j <= ctvercu_hloubka; j++)
                {
                    if (first_rule)
                    {
                        if ((matice[j, i, 2] == 0) && (matice[j, i, 3] < 2))
                        {
                            matice[j, i, 2] = 1;
                            matice[j, i, 3] = 0;
                            Draw(i, j, color_third);        //1
                            continue;
                        }
                    }

                    if (second_rule)
                    {
                        if ((matice[j, i, 3] > 3) && (matice[j, i, 2] == 0))
                        {
                            matice[j, i, 2] = 1;
                            matice[j, i, 3] = 0;
                            Draw(i, j, color_fourth);        //1
                            continue;
                        }
                    }

                    if (fourth_rule)
                    {
                        if ((matice[j, i, 2] == 1) && (matice[j, i, 3] == 3))
                        {
                            matice[j, i, 2] = 0;
                            matice[j, i, 3] = 0;
                            Draw(i, j, color_first);        //0
                            continue;
                        }
                    }

                    if (third_rule)
                    {
                        if ((matice[j, i, 2] == 0) && ((matice[j, i, 3] == 2) || (matice[j, i, 3] == 3)))
                        {
                            matice[j, i, 2] = 0;
                            matice[j, i, 3] = 0;
                            Draw(i, j, color_second);      //0
                            continue;
                        }
                    }

                    matice[j, i, 3] = 0;
                    
                }
            }
        }

        private void Draw(int x, int y,string barva)
        {
            
            DrawingVisual drawingVisual2 = new DrawingVisual();
           
            if (buffer == null)
                return;

            using (DrawingContext drawingContext = drawingVisual2.RenderOpen())
            {
                drawingContext.DrawRectangle(new SolidColorBrush((Color)ColorConverter.ConvertFromString(barva)), null, new Rect(x*12, y*12, 10, 10));                        
            }

            buffer.Render(drawingVisual2);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rychlost = (int)rychlos_Slider.Value * 100;
            rychlost_Label.Content = "Rychlost: " + rychlost;           
        }

        private void Reset_matice()
        {
            for (int i = 0; i <= ctvercu_sirka; i++)
            {
                for (int j = 0; j <= ctvercu_hloubka; j++)
                {
                    matice[j, i, 2] = 1;
                    matice[j, i, 3] = 0;
                }
            }
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (konfigurace == false) return;

            if (bezi)
            {
                first_rule = first_ruleCeck.IsChecked.Value;
                second_rule = second_ruleCeck.IsChecked.Value;
                third_rule = third_ruleCeck.IsChecked.Value;
                fourth_rule = fourth_ruleCeck.IsChecked.Value;
                dispatcherTimer.Stop();
                Reset_matice();
                buffer = new RenderTargetBitmap((int)Background.Width, (int)Background.Height, 96, 96, PixelFormats.Pbgra32);
                Background.Source = buffer;
                DrawStuff();
                kol = 0;
                SelectRandom(userVal);                
                rychlost = (int)rychlos_Slider.Value * 100;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, rychlost);
                bezi = true;
                dispatcherTimer.Start();
            }
            else
            {
                first_rule = first_ruleCeck.IsChecked.Value;
                second_rule = second_ruleCeck.IsChecked.Value;
                third_rule = third_ruleCeck.IsChecked.Value;
                fourth_rule = fourth_ruleCeck.IsChecked.Value;
                Start_Button.Content = "Restart";
                rychlost = (int)rychlos_Slider.Value * 100;
                dispatcherTimer.Tick += Kolo;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, rychlost);
                bezi = true;
                dispatcherTimer.Start();
            }     
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            color_first = ClrPcker_Background.SelectedColor.ToString();
        }

        private void Color_Select(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            color_second = ClrPcker_Select.SelectedColor.ToString();
        }

        private void Color_Select2(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            color_third = ClrPcker_Select2.SelectedColor.ToString();
        }

        private void Color_Select3(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            color_fourth = ClrPcker_Select3.SelectedColor.ToString();
        }
    }
}