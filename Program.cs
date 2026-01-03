using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GraphApp
{
    // Головний клас програми
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Запуск нашої форми
            Application.Run(new GraphForm());
        }
    }

    // Клас форми для малювання графіка
    public class GraphForm : Form
    {
        // Константи умови задачі
        private const double Xmin = -1.0;
        private const double Xmax = 2.3;
        private const double Dx = 0.7;

        // Налаштування відступів
        private const int MarginVal = 60;

        public GraphForm()
        {
            // Налаштування властивостей форми
            this.Text = "Графік y = (e^(2x) - 8) / (x + 3)";
            this.ClientSize = new Size(900, 600);
            this.BackColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Вмикаємо перерисовку при зміні розмірів вікна
            this.ResizeRedraw = true;

            // Подвійна буферизація для усунення мерехтіння
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.UserPaint | 
                          ControlStyles.OptimizedDoubleBuffer, true);
        }

        // Метод обчислення функції
        private double CalcFunc(double x)
        {
            // y = (e^(2x) - 8) / (x + 3)
            return (Math.Exp(2 * x) - 8) / (x + 3);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Розміри області малювання
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            // 1. Знаходимо min і max Y для масштабування
            // Проходимо частими кроками, щоб знайти точні піки
            double yMin = double.MaxValue;
            double yMax = double.MinValue;
            
            int steps = 1000;
            for (int i = 0; i <= steps; i++)
            {
                double x = Xmin + (Xmax - Xmin) * i / steps;
                double y = CalcFunc(x);
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            // Запобігаємо діленню на нуль, якщо графік горизонтальний
            if (Math.Abs(yMax - yMin) < 1e-9)
            {
                yMin -= 1.0;
                yMax += 1.0;
            }

            // 2. Коефіцієнти масштабування
            // Ширина та висота області графіка (без відступів)
            double plotWidth = w - 2 * MarginVal;
            double plotHeight = h - 2 * MarginVal;

            double scaleX = plotWidth / (Xmax - Xmin);
            double scaleY = plotHeight / (yMax - yMin);

            // 3. Інструменти для малювання
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            using (Pen axisPen = new Pen(Color.Black, 2))
            using (Pen graphPen = new Pen(Color.Blue, 2))
            using (Brush pointBrush = new SolidBrush(Color.Red))
            using (Font font = new Font("Arial", 10))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                // -- Малюємо осі --
                // Ліва вертикальна лінія
                g.DrawLine(axisPen, MarginVal, MarginVal, MarginVal, h - MarginVal);
                // Нижня горизонтальна лінія
                g.DrawLine(axisPen, MarginVal, h - MarginVal, w - MarginVal, h - MarginVal);

                // Підписи осей
                g.DrawString("Y", font, textBrush, MarginVal - 10, MarginVal - 20);
                g.DrawString("X", font, textBrush, w - MarginVal + 10, h - MarginVal - 10);

                // Підписи значень (Min/Max)
                g.DrawString(Xmin.ToString("0.0"), font, textBrush, MarginVal, h - MarginVal + 5);
                g.DrawString(Xmax.ToString("0.0"), font, textBrush, w - MarginVal - 20, h - MarginVal + 5);
                
                g.DrawString(yMin.ToString("0.00"), font, textBrush, 5, h - MarginVal - 10);
                g.DrawString(yMax.ToString("0.00"), font, textBrush, 5, MarginVal);


                // -- Малюємо сам графік (плавна лінія) --
                PointF? prevPoint = null;
                // Малюємо лінію з високою деталізацією (по пікселях)
                for (int i = 0; i <= plotWidth; i++)
                {
                    // Переводимо піксель i назад у координату X
                    double xReal = Xmin + (i / scaleX);
                    if (xReal > Xmax) break;

                    double yReal = CalcFunc(xReal);

                    // Переводимо координати у екранні
                    float xScreen = (float)(MarginVal + (xReal - Xmin) * scaleX);
                    // Y інвертуємо, бо на екрані 0 зверху
                    float yScreen = (float)(h - MarginVal - (yReal - yMin) * scaleY);

                    PointF currentPoint = new PointF(xScreen, yScreen);

                    if (prevPoint != null)
                    {
                        g.DrawLine(graphPen, prevPoint.Value, currentPoint);
                    }
                    prevPoint = currentPoint;
                }

                // -- Малюємо контрольні точки з кроком Dx = 0.7 --
                // Використовуємо 1e-5 для компенсації похибки float
                for (double x = Xmin; x <= Xmax + 1e-5; x += Dx)
                {
                    double y = CalcFunc(x);

                    float xScreen = (float)(MarginVal + (x - Xmin) * scaleX);
                    float yScreen = (float)(h - MarginVal - (y - yMin) * scaleY);

                    // Малюємо точку
                    float r = 3; // радіус точки
                    g.FillEllipse(pointBrush, xScreen - r, yScreen - r, 2 * r, 2 * r);
                    
                    // Можна додати підпис значення біля точки (за бажанням)
                    // g.DrawString($"{y:0.0}", font, textBrush, xScreen, yScreen - 20);
                }
            }
        }
    }
}
