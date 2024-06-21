using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Doanhocphan1
{
    public partial class Form1 : Form
    {
        private float speed = 0.0f; // Tốc độ ban đầu

        public Form1()
        {
            InitializeComponent();
            lbSpeed.Text = "Speed: X" + speed.ToString();
            this.Resize += new EventHandler(Form1_Resize); // Thêm sự kiện Resize
        }

        Thread th;
        Graphics g;
        Graphics fG;
        Bitmap bmp;

        bool drawing = true;

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            th = new Thread(Draw);
            th.IsBackground = true;
            th.Start();
        }

        private void InitializeGraphics()
        {
            bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            g = Graphics.FromImage(bmp);
            fG = CreateGraphics();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            InitializeGraphics(); // Khởi tạo lại đồ họa với kích thước mới
        }

        private void Draw()
        {
            float angle = 270;
            float crankRadius = 80;  // Bán kính trục khuỷu (r)

            Pen p = new Pen(Brushes.Azure, 3.0f);
            Pen centerPen = new Pen(Brushes.Yellow, 1.0f); // Bút vẽ tâm piston
            Pen pistonPen = new Pen(Brushes.White, 3.0f); // Bút vẽ xi lanh

            while (drawing)
            {
                float centerX = this.ClientSize.Width / 2;
                float centerY = this.ClientSize.Height / 2;
                PointF org = new PointF(centerX - 200, centerY);  // Tâm của đường tròn (trục khuỷu)

                RectangleF area = new RectangleF(org.X - crankRadius, org.Y - crankRadius, 2 * crankRadius, 2 * crankRadius);
                RectangleF circle = new RectangleF(0, 0, 10, 10);

                float pistonHeight = 80; // Chiều cao piston
                float pistonWidth = 50;  // Chiều rộng piston
                float cylinderWidth = 230;  // Chiều dài xi-lanh 
                float cylinderHeight = 80;  // Chiều cao xi-lanh
                float cylinderX = centerX + crankRadius;
                float cylinderY = centerY - cylinderHeight / 2;

                float minPistonX = cylinderX; // Giới hạn tọa độ X tối thiểu của piston
                float maxPistonX = cylinderX + cylinderWidth - pistonWidth; // Giới hạn tọa độ X tối đa của piston

                float pistonX = minPistonX; // Khởi tạo tọa độ X của piston
                PointF loc = PointF.Empty; // Điểm trên đường tròn mà trục khuỷu di chuyển tới trong mỗi khung hình

                g.Clear(Color.Black);
                g.DrawEllipse(p, area); // Vẽ đường tròn

                // Vẽ tâm đường tròn
                g.FillEllipse(Brushes.Yellow, org.X - 5, org.Y - 5, 10, 10);

                // Tính vị trí của điểm loc trên đường tròn
                loc = CirclePoint(crankRadius, angle, org);
                circle.X = loc.X - (circle.Width / 2);
                circle.Y = loc.Y - (circle.Width / 2);

                // Vẽ điểm di chuyển (loc)
                g.FillEllipse(Brushes.Red, circle);

                // Tính vị trí điểm đối diện của loc qua tâm đường tròn
                PointF oppositeLoc = CirclePoint(crankRadius, angle + 180, org);

                // Vẽ điểm đối diện của loc
                RectangleF oppositeCircle = new RectangleF(oppositeLoc.X - (circle.Width / 2), oppositeLoc.Y - (circle.Width / 2), 10, 10);
                g.FillEllipse(Brushes.Red, oppositeCircle);

                // Vẽ đường kính qua tâm
                g.DrawLine(centerPen, loc, oppositeLoc);

                // Tính vị trí của piston dựa trên vị trí của loc
                pistonX = cylinderX + (loc.X - org.X) + crankRadius;

                // Di chuyển piston và kiểm tra giới hạn
                if (pistonX < minPistonX || pistonX > maxPistonX)
                {
                    pistonX = Math.Max(minPistonX, Math.Min(pistonX, maxPistonX)); // Giữ piston trong phạm vi của xi-lanh
                }

                // Tính toán tọa độ của cạnh bên trái của piston
                float pistonLeftEdgeX = pistonX;
                float pistonLeftEdgeY = cylinderY + (cylinderHeight - pistonHeight) / 2; // Trung điểm theo chiều Y

                // Tính toán trung điểm của cạnh bên trái của piston
                PointF pistonCenter = new PointF(pistonLeftEdgeX + pistonWidth / 2, pistonLeftEdgeY + pistonHeight / 2);

                // Vẽ thanh nối & điểm tiếp xúc với piston
                g.DrawLine(p, loc, pistonCenter);
                g.FillEllipse(Brushes.Red, pistonCenter.X - 5, pistonCenter.Y - 5, 10, 10);

                // Vẽ piston
                g.FillRectangle(Brushes.White, pistonLeftEdgeX, pistonLeftEdgeY, pistonWidth, pistonHeight);

                // Vẽ các cạnh của xi-lanh bằng cách vẽ từng đoạn thẳng
                PointF topLeft = new PointF(cylinderX, cylinderY);
                PointF topRight = new PointF(cylinderX + cylinderWidth, cylinderY);
                PointF bottomLeft = new PointF(cylinderX, cylinderY + cylinderHeight);
                PointF bottomRight = new PointF(cylinderX + cylinderWidth, cylinderY + cylinderHeight);

                g.DrawLine(pistonPen, topLeft, topRight); // Cạnh trên
                g.DrawLine(pistonPen, bottomLeft, bottomRight); // Cạnh dưới
                g.DrawLine(pistonPen, topRight, bottomRight); // Cạnh phải

                // Vẽ hai đoạn thẳng vuông góc ở đầu bên trái của xi-lanh
                PointF topLeftVertical = new PointF(cylinderX, cylinderY - 20); // Điểm trên cạnh trên hướng lên trên
                PointF bottomLeftVertical = new PointF(cylinderX, cylinderY + cylinderHeight + 20); // Điểm trên cạnh dưới hướng xuống dưới
                g.DrawLine(pistonPen, topLeft, topLeftVertical); // Đoạn thẳng trên hướng lên trên
                g.DrawLine(pistonPen, bottomLeft, bottomLeftVertical); // Đoạn thẳng dưới hướng xuống dưới

                fG.DrawImage(bmp, 0, 0);

                angle += speed * 2.0f; // Tăng góc quay của trục khuỷu theo tốc độ
                if (angle >= 360)
                {
                    angle = 0;
                }

                Thread.Sleep(10); // Đợi 10ms trước khi vẽ khung hình tiếp theo
            }
        }

        public PointF CirclePoint(float radius, float angleInDegrees, PointF origin)
        {
            // Chuyển góc từ toạ độ sang rad
            float angleInRadians = (float)(angleInDegrees * (Math.PI / 180));

            // Tính toán tọa độ x và y của điểm trên đường tròn
            float x = origin.X + (float)(radius * Math.Cos(angleInRadians));
            float y = origin.Y + (float)(radius * Math.Sin(angleInRadians));

            return new PointF(x, y);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                speed += 1.0f; // Tăng tốc độ lên 1.0f
                lbSpeed.Text =  "Speed: X" + speed.ToString(); // Cập nhật hiển thị tốc độ
                return true; // Chỉ ra rằng phím đã được xử lý
            }
            else if (keyData == Keys.Down)
            {
                speed = Math.Max(0.0f, speed - 1.0f); // Giảm tốc độ xuống 1.0f, nhưng không dưới 0.0f
                lbSpeed.Text = "Speed: X" + speed.ToString();
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
