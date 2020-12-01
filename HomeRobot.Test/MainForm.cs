using HomeRobot.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeRobot.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Application.Idle += Application_Idle;
        
            mnuStrategy.Items.AddRange(new[] { typeof(RandomRobot), typeof(CleanerRobot), typeof(CatcherRobot), typeof(SmartRobot) });
            mnuStrategy.SelectedIndex = 0;
        }

        private const int cellSize = 64; //pixels
        private Bitmap image;

        private EnvironmentSettings[] settings;
        private Core.Environment environment;
        private Simulation simulation;
        private bool inStep;
        private int stepCount;

        //Leer las condiciones iniciales del ambiente de un archivo json
        private void GetInitialConditions(string filename)
        {
            var s = File.ReadAllText(filename);
            settings = JsonConvert.DeserializeObject<EnvironmentSettings[]>(s);
        }

        private void CreateSimulation(EnvironmentSettings settings, Type robotType)
        {
            stepCount = 0;

            environment = new Core.Environment(settings, robotType);
            environment.Played += Environment_Played;
            environment.Robot.Playing += Robot_Playing;
            environment.Robot.Played += Robot_Played;
            environment.Robot.Moving += Robot_Moving;
            foreach(var child in environment.Children)
            {
                child.Playing += Child_Playing;
                child.Played += Child_Played;
                child.Moving += Child_Moving;
            }

            simulation = new Simulation(environment, environment.Robot);

            SetImageSize((settings.Cols + 1) * (cellSize + 1) + 2, (settings.Rows + 1)* (cellSize + 1) + 2);
            DrawImage();
            UpdateDirtyPercent();
            UpdateStepCounter();
            pbcEnv.Invalidate();
        }

        private void Environment_Played(object sender, EventArgs e)
        {
            DrawImage();
            UpdateDirtyPercent();
            pbcEnv.Invalidate();
        }

        private void Robot_Moving(object sender, MovingArgs e)
        {
            DrawSelectedCell(e.NewPosition, Color.Yellow);
            pbcEnv.Invalidate();
            Application.DoEvents();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void Robot_Playing(object sender, EventArgs e)
        {
            var robot = (Core.HomeRobot)sender;
            DrawSelectedCell(robot.CurrentPos, Color.Red);
            pbcEnv.Invalidate();
            Application.DoEvents();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void Robot_Played(object sender, EventArgs e)
        {
            DrawImage();
            UpdateDirtyPercent();
            pbcEnv.Invalidate();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void Child_Moving(object sender, MovingArgs e)
        {
            DrawSelectedCell(e.NewPosition, Color.Yellow);
            pbcEnv.Invalidate();
            Application.DoEvents();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void Child_Playing(object sender, EventArgs e)
        {
            var child = (Child)sender;
            DrawImage();
            DrawSelectedCell(child.CurrentPos, Color.Red);
            pbcEnv.Invalidate();
            Application.DoEvents();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void Child_Played(object sender, EventArgs e)
        {
            DrawImage();
            UpdateDirtyPercent();
            pbcEnv.Invalidate();
            Application.DoEvents();
            if (!tmrSimulation.Enabled)
                Thread.Sleep(1000);
        }

        private void SetImageSize(int width, int height)
        {
            image = new Bitmap(width, height);
            pbcEnv.ClientSize = new Size(image.Width, image.Height);
        }

        private void UpdateDirtyPercent()
        {
            pbrDirtyPercent.Value = (int)environment.DirtPercent;
            lblDirtyPercentVal.Text = pbrDirtyPercent.Value.ToString();
        }
        private void UpdateStepCounter()
        {
            lblStepCounterVal.Text = stepCount.ToString();
        }


        private void DrawImage()
        {
            using (var graphics = System.Drawing.Graphics.FromImage(image))
            {
                graphics.FillRectangle(new SolidBrush(pbcEnv.BackColor), 0, 0, image.Width, image.Height);

                var delta = cellSize + 1;

                var pen = new Pen(Color.Black);
                for (var x = delta; x < image.Width; x += delta)
                    graphics.DrawLine(pen, x, delta, x, image.Height);
                for (var y = delta; y < image.Height; y += delta)
                    graphics.DrawLine(pen, delta, y, image.Width, y);

                var brush = new SolidBrush(Color.Black);
                for (var row = 0; row < environment.Dimensions.X; row++)
                {
                    var x1 = 0;
                    var x2 = delta;
                    var y1 = (row + 1) * delta;
                    var y2 = (row + 2) * delta;
                    graphics.DrawString(row.ToString(), Font, brush, 
                        Rectangle.FromLTRB(x1, y1, x2, y2),
                        new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
                for (var col = 0; col < environment.Dimensions.Y; col++)
                {
                    var x1 = (col + 1) * delta;
                    var x2 = (col + 2) * delta;
                    var y1 = 0;
                    var y2 = delta;
                    graphics.DrawString(col.ToString(), Font, brush,
                        Rectangle.FromLTRB(x1, y1, x2, y2),
                        new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                for (var row = 0; row < environment.Dimensions.X; row++)
                {
                    var y1 = (row + 1)* delta;
                    var y2 = (row + 2) * delta;
                    for (var col = 0; col < environment.Dimensions.Y; col++)
                    {
                        var x1 = (col + 1)* delta;
                        var x2 = (col + 2) * delta;

                        var stateName = "";
                        var separator = "";
                        if (environment[row, col].IsObstacled())
                        {
                            stateName = "Obstacle";
                        }
                        else
                        {
                            if (environment[row, col].IsRobot())
                            {
                                stateName = "Robot";
                                separator = "_";
                                if (((Core.HomeRobot)environment[row, col].GetMobileObject(CellStatus.Robot)).IsCarryingChild())
                                    stateName += separator + "Carrying_Child";
                            }
                            if (environment[row, col].IsChild())
                            {
                                stateName += separator + "Child";
                                separator = "_";
                            }
                            if (environment[row, col].IsPlaypen())
                            {
                                stateName += separator + "Playpen";
                                separator = "_";
                            }
                            if (environment[row, col].IsDirty())
                            {
                                stateName += separator + "Dirt";
                                separator = "_";
                            }
                        }

                        if (string.IsNullOrEmpty(stateName))
                            continue;

                        var stateFilename = Path.ChangeExtension(Path.Combine("Images", stateName), "png");
                        using (var cellImage = Image.FromFile(stateFilename))
                        {
                            graphics.DrawImage(
                                cellImage,
                                x1, y1, cellSize, cellSize);
                        }

                    }
                }
            }
        }

        private void DrawSelectedCell(Point currentPos, Color color)
        {
            using (var graphics = System.Drawing.Graphics.FromImage(image))
            {
                var delta = cellSize + 1;

                var pen = new Pen(color, 3);
                var y1 = (currentPos.X + 1) * delta;
                var y2 = (currentPos.X + 2) * delta;
                var x1 = (currentPos.Y + 1) * delta;
                var x2 = (currentPos.Y + 2) * delta;

                graphics.DrawRectangle(pen, Rectangle.FromLTRB(x1, y1, x2, y2));
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            mnuNewSimulation.Enabled = mnuSettings.SelectedItem != null && !tmrSimulation.Enabled;
            mnuStart.Enabled = simulation != null && !(simulation.Successfull() || simulation.RobotDissmissed()) && !tmrSimulation.Enabled;
            mnuStop.Enabled = simulation != null && tmrSimulation.Enabled;
            mnuStep.Enabled = simulation != null && !(simulation.Successfull() || simulation.RobotDissmissed()) && !tmrSimulation.Enabled;
        }

        private void CmExit(object sender, EventArgs e)
        {
            Close();
        }

        private void CmOpenConfig(object sender, EventArgs e)
        {
            if (dlgConfig.ShowDialog() == DialogResult.OK)
            {
                GetInitialConditions(dlgConfig.FileName);
                mnuSettings.Items.Clear();
                mnuSettings.Items.AddRange(settings);
                mnuSettings.SelectedIndex = 0;
            }
        }

        private void CmNewSimulation(object sender, EventArgs e)
        {
            CreateSimulation((EnvironmentSettings)mnuSettings.SelectedItem, (Type)mnuStrategy.SelectedItem);
        }

        private void pbcEnvPaint(object sender, PaintEventArgs e)
        {
            if (image != null)
                e.Graphics.DrawImage(image, 0, 0);
        }

        private void mnuSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            CmNewSimulation(mnuNewSimulation, null);
        }

        private void CmStep(object sender, EventArgs e)
        {
            DoStep();
        }

        private void DoStep()
        {
            if (inStep)
                return;
            try
            {
                inStep = true;
                simulation.PlayTurn();
                ++stepCount;
                UpdateStepCounter();
                if (simulation.Successfull())
                    MessageBox.Show("Robot finished successfully!!");
                else if (simulation.RobotDissmissed())
                    MessageBox.Show("Robot Dissmissed!!");
                else if (stepCount > Core.Environment.MaxEnvironmentChanges * environment.TurnsToChange)
                    MessageBox.Show("Simulation timeout!!");
                else
                    return;
                tmrSimulation.Enabled = false;
            }
            finally
            {
                inStep = false;
            }
        }

        private void tmrSimulation_Tick(object sender, EventArgs e)
        {
            DoStep();
        }

        private void CmStartStop(object sender, EventArgs e)
        {
            tmrSimulation.Enabled = !tmrSimulation.Enabled;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = tmrSimulation.Enabled || inStep;
        }
    }
}
