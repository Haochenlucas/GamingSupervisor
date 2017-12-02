using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GamingSupervisor
{
    public partial class Form1 : Form
    {
        public string difficulty;        

        public Boolean hero_selection    = false;
        public Boolean item_helper       = false;
        public Boolean laning            = false;
        public Boolean last_hit          = false;
        public Boolean jungling          = false;
        public Boolean safe_farming_area = false;

        public Boolean replay_selected = false;

        public enum State
        {
            select_difficulty,
            customize,
            game_type,
            start
        };

        public State state = State.select_difficulty;

        public string filename;

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            checkbox_container.Left = (this.ClientSize.Width - checkbox_container.Width) / 2;
            checkbox_container.Top  = (this.ClientSize.Height - checkbox_container.Height) / 2;
            checkbox_container.Hide();
            
            title_label.Left = (this.ClientSize.Width - title_label.Width) / 2;

            novice_button.Left = (this.ClientSize.Width - novice_button.Width) / 2;
            novice_button.Top  = (this.ClientSize.Height - novice_button.Height) / 2 - 100;

            learning_button.Left = (this.ClientSize.Width - novice_button.Width) / 2;
            learning_button.Top  = (this.ClientSize.Height - novice_button.Height) / 2;

            almost_button.Left = (this.ClientSize.Width - novice_button.Width) / 2;
            almost_button.Top  = (this.ClientSize.Height - novice_button.Height) / 2 + 100;

            player_level_text.Left = title_label.Left;
            player_level.Left      = player_level_text.Right;

            replay_button.Left = (this.ClientSize.Width - replay_button.Width) / 2;
            replay_button.Top  = (this.ClientSize.Height + player_level.Bottom - replay_button.Width) / 2 - 50;

            live_button.Left = (this.ClientSize.Width - live_button.Width) / 2;
            live_button.Top  = (this.ClientSize.Height + player_level.Bottom - live_button.Width) / 2 + 50;

            go_button.Left = (this.ClientSize.Width - go_button.Width) / 2;
            go_button.Top  = (this.ClientSize.Height + player_level.Bottom - go_button.Width) / 2;

            timer_text.Left = (this.ClientSize.Width - go_button.Width) / 2;
            timer_text.Top  = (this.ClientSize.Height + player_level.Bottom - go_button.Width) / 2;
        }
        
        private void novice_button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            difficulty = btn.Text;
            btn.Hide();
            learning_button.Hide();
            almost_button.Hide();
            checkbox_container.Show();
            player_level.Text += difficulty;
            player_level_text.Show();
            player_level.Show();

            cb_confirm.Show();
            back_button.Show();

            state = State.customize;
        }

        private void learning_button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            difficulty = btn.Text;
            btn.Hide();
            novice_button.Hide();
            almost_button.Hide();
            checkbox_container.Show();
            player_level.Text += difficulty;
            player_level_text.Show();
            player_level.Show();

            cb_confirm.Show();
            back_button.Show();

            state = State.customize;
        }

        private void almost_button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            difficulty = btn.Text;
            btn.Hide();
            novice_button.Hide();
            learning_button.Hide();
            checkbox_container.Show();
            player_level.Text += difficulty;
            player_level_text.Show();
            player_level.Show();

            cb_confirm.Show();
            back_button.Show();

            state = State.customize;
        }

        private void lh_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            last_hit = cb.Checked;
        }

        private void hs_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            hero_selection = cb.Checked;
        }

        private void ih_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            item_helper = cb.Checked;
        }

        private void ln_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            laning = cb.Checked;
        }

        private void jg_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            jungling = cb.Checked;
        }

        private void sfa_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            safe_farming_area = cb.Checked;
        }
        
        private void cb_confirm_Click(object sender, EventArgs e)
        {
            if (!(hero_selection || item_helper || laning || last_hit || jungling || safe_farming_area))
            {
                MessageBox.Show("Select at least one option!");
            }
            else
            {
                state = State.game_type;
                checkbox_container.Hide();
                cb_confirm.Hide();

                replay_button.Show();
                live_button.Show();
            }
        }

        private void back_button_Click(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.customize:
                    cb_confirm.Hide();
                    back_button.Hide();
                    checkbox_container.Hide();
                    player_level.Hide();
                    player_level_text.Hide();

                    novice_button.Show();
                    learning_button.Show();
                    almost_button.Show();

                    state = State.select_difficulty;
                    break;
                case State.game_type:
                    cb_confirm.Show();
                    checkbox_container.Show();

                    replay_button.Hide();
                    live_button.Hide();

                    state = State.customize;
                    break;
                case State.start:
                    go_button.Hide();

                    replay_button.Show();
                    live_button.Show();

                    state = State.game_type;
                    break;
            }
        }

        private void replay_button_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "dem files (*.dem)|*.dem";
            openFileDialog1.Title = "Select a replay file";
            DialogResult result = openFileDialog1.ShowDialog();

            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            state = State.start;

            replay_button.Hide();
            live_button.Hide();

            go_button.Show();

            replay_selected = true;
        }

        public int timeleft = 5;

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            OpenFileDialog ofd = sender as OpenFileDialog;
            filename = ofd.FileName;
        }

        private void parseReplayFile()
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute        = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName               = "java";
            p.StartInfo.Arguments =        
                "-jar " 
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\parser.jar ")
                + filename.Replace(@"\", @"\\")
                + " "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\"); // Data dump location
            p.Start();

            //String s = p.StandardOutput.ReadToEnd();
            //Console.Write(s);
            p.WaitForExit();
            Console.WriteLine("Finished parsing!");
        }

        private void go_button_Click(object sender, EventArgs e)
        {
            go_button.Hide();
            timer_text.Show();
            back_button.Hide();

            Thread thread = new Thread(parseReplayFile);
            thread.Start();

            timer1.Start();

            //uthread.Join();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(timeleft > 0)
            {
                timeleft--;
                timer_text.Text = timeleft + "";
            }
            else
            {
                timer1.Stop();
                timer_text.Text = "0";
            }
        }
    }
}
