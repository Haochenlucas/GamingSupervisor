namespace GamingSupervisor
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.title_label = new System.Windows.Forms.Label();
            this.novice_button = new System.Windows.Forms.Button();
            this.learning_button = new System.Windows.Forms.Button();
            this.almost_button = new System.Windows.Forms.Button();
            this.hs_checkbox = new System.Windows.Forms.CheckBox();
            this.ih_checkbox = new System.Windows.Forms.CheckBox();
            this.ln_checkbox = new System.Windows.Forms.CheckBox();
            this.lh_checkbox = new System.Windows.Forms.CheckBox();
            this.jg_checkbox = new System.Windows.Forms.CheckBox();
            this.sfa_checkbox = new System.Windows.Forms.CheckBox();
            this.checkbox_container = new System.Windows.Forms.FlowLayoutPanel();
            this.player_level_text = new System.Windows.Forms.Label();
            this.player_level = new System.Windows.Forms.Label();
            this.cb_confirm = new System.Windows.Forms.Button();
            this.back_button = new System.Windows.Forms.Button();
            this.replay_button = new System.Windows.Forms.Button();
            this.live_button = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.go_button = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer_text = new System.Windows.Forms.Label();
            this.hero_select_label = new System.Windows.Forms.Label();
            this.hero_select_box = new System.Windows.Forms.ComboBox();
            this.hero_select_button = new System.Windows.Forms.Button();
            this.tick_timer = new System.Windows.Forms.Timer(this.components);
            this.parsing_label = new System.Windows.Forms.Label();
            this.checkbox_container.SuspendLayout();
            this.SuspendLayout();
            // 
            // title_label
            // 
            this.title_label.AutoSize = true;
            this.title_label.Cursor = System.Windows.Forms.Cursors.Default;
            this.title_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.title_label.Location = new System.Drawing.Point(43, 9);
            this.title_label.Name = "title_label";
            this.title_label.Size = new System.Drawing.Size(243, 25);
            this.title_label.TabIndex = 0;
            this.title_label.Text = "GAMING SUPERVISOR";
            // 
            // novice_button
            // 
            this.novice_button.Location = new System.Drawing.Point(96, 103);
            this.novice_button.Name = "novice_button";
            this.novice_button.Size = new System.Drawing.Size(100, 50);
            this.novice_button.TabIndex = 1;
            this.novice_button.Text = "Novice";
            this.novice_button.UseVisualStyleBackColor = true;
            this.novice_button.Click += new System.EventHandler(this.novice_button_Click);
            // 
            // learning_button
            // 
            this.learning_button.Location = new System.Drawing.Point(176, 266);
            this.learning_button.Name = "learning_button";
            this.learning_button.Size = new System.Drawing.Size(100, 50);
            this.learning_button.TabIndex = 2;
            this.learning_button.Text = "Learning";
            this.learning_button.UseVisualStyleBackColor = true;
            this.learning_button.Click += new System.EventHandler(this.learning_button_Click);
            // 
            // almost_button
            // 
            this.almost_button.Location = new System.Drawing.Point(176, 322);
            this.almost_button.Name = "almost_button";
            this.almost_button.Size = new System.Drawing.Size(100, 50);
            this.almost_button.TabIndex = 0;
            this.almost_button.Text = "Almost got it";
            this.almost_button.UseVisualStyleBackColor = true;
            this.almost_button.Click += new System.EventHandler(this.almost_button_Click);
            // 
            // hs_checkbox
            // 
            this.hs_checkbox.AutoSize = true;
            this.hs_checkbox.Location = new System.Drawing.Point(3, 26);
            this.hs_checkbox.Name = "hs_checkbox";
            this.hs_checkbox.Size = new System.Drawing.Size(96, 17);
            this.hs_checkbox.TabIndex = 3;
            this.hs_checkbox.Text = "Hero Selection";
            this.hs_checkbox.UseVisualStyleBackColor = true;
            this.hs_checkbox.CheckedChanged += new System.EventHandler(this.hs_checkbox_CheckedChanged);
            // 
            // ih_checkbox
            // 
            this.ih_checkbox.AutoSize = true;
            this.ih_checkbox.Location = new System.Drawing.Point(3, 49);
            this.ih_checkbox.Name = "ih_checkbox";
            this.ih_checkbox.Size = new System.Drawing.Size(80, 17);
            this.ih_checkbox.TabIndex = 4;
            this.ih_checkbox.Text = "Item Helper";
            this.ih_checkbox.UseVisualStyleBackColor = true;
            this.ih_checkbox.CheckedChanged += new System.EventHandler(this.ih_checkbox_CheckedChanged);
            // 
            // ln_checkbox
            // 
            this.ln_checkbox.AutoSize = true;
            this.ln_checkbox.Location = new System.Drawing.Point(3, 72);
            this.ln_checkbox.Name = "ln_checkbox";
            this.ln_checkbox.Size = new System.Drawing.Size(58, 17);
            this.ln_checkbox.TabIndex = 5;
            this.ln_checkbox.Text = "Laning";
            this.ln_checkbox.UseVisualStyleBackColor = true;
            this.ln_checkbox.CheckedChanged += new System.EventHandler(this.ln_checkbox_CheckedChanged);
            // 
            // lh_checkbox
            // 
            this.lh_checkbox.AutoSize = true;
            this.lh_checkbox.Location = new System.Drawing.Point(3, 3);
            this.lh_checkbox.Name = "lh_checkbox";
            this.lh_checkbox.Size = new System.Drawing.Size(79, 17);
            this.lh_checkbox.TabIndex = 6;
            this.lh_checkbox.Text = "Last Hitting";
            this.lh_checkbox.UseVisualStyleBackColor = true;
            this.lh_checkbox.CheckedChanged += new System.EventHandler(this.lh_checkbox_CheckedChanged);
            // 
            // jg_checkbox
            // 
            this.jg_checkbox.AutoSize = true;
            this.jg_checkbox.Location = new System.Drawing.Point(67, 72);
            this.jg_checkbox.Name = "jg_checkbox";
            this.jg_checkbox.Size = new System.Drawing.Size(65, 17);
            this.jg_checkbox.TabIndex = 7;
            this.jg_checkbox.Text = "Jungling";
            this.jg_checkbox.UseVisualStyleBackColor = true;
            this.jg_checkbox.CheckedChanged += new System.EventHandler(this.jg_checkbox_CheckedChanged);
            // 
            // sfa_checkbox
            // 
            this.sfa_checkbox.AutoSize = true;
            this.sfa_checkbox.Location = new System.Drawing.Point(3, 95);
            this.sfa_checkbox.Name = "sfa_checkbox";
            this.sfa_checkbox.Size = new System.Drawing.Size(113, 17);
            this.sfa_checkbox.TabIndex = 8;
            this.sfa_checkbox.Text = "Safe Farming Area";
            this.sfa_checkbox.UseVisualStyleBackColor = true;
            this.sfa_checkbox.CheckedChanged += new System.EventHandler(this.sfa_checkbox_CheckedChanged);
            // 
            // checkbox_container
            // 
            this.checkbox_container.Controls.Add(this.lh_checkbox);
            this.checkbox_container.Controls.Add(this.hs_checkbox);
            this.checkbox_container.Controls.Add(this.ih_checkbox);
            this.checkbox_container.Controls.Add(this.ln_checkbox);
            this.checkbox_container.Controls.Add(this.jg_checkbox);
            this.checkbox_container.Controls.Add(this.sfa_checkbox);
            this.checkbox_container.Location = new System.Drawing.Point(89, 95);
            this.checkbox_container.Name = "checkbox_container";
            this.checkbox_container.Size = new System.Drawing.Size(141, 269);
            this.checkbox_container.TabIndex = 9;
            // 
            // player_level_text
            // 
            this.player_level_text.AutoSize = true;
            this.player_level_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.player_level_text.Location = new System.Drawing.Point(45, 47);
            this.player_level_text.Name = "player_level_text";
            this.player_level_text.Size = new System.Drawing.Size(85, 13);
            this.player_level_text.TabIndex = 10;
            this.player_level_text.Text = "Player Level: ";
            this.player_level_text.Visible = false;
            // 
            // player_level
            // 
            this.player_level.AutoSize = true;
            this.player_level.Location = new System.Drawing.Point(132, 47);
            this.player_level.Name = "player_level";
            this.player_level.Size = new System.Drawing.Size(0, 13);
            this.player_level.TabIndex = 11;
            this.player_level.Visible = false;
            // 
            // cb_confirm
            // 
            this.cb_confirm.Location = new System.Drawing.Point(13, 412);
            this.cb_confirm.Name = "cb_confirm";
            this.cb_confirm.Size = new System.Drawing.Size(75, 32);
            this.cb_confirm.TabIndex = 12;
            this.cb_confirm.Text = "Confirm";
            this.cb_confirm.UseVisualStyleBackColor = true;
            this.cb_confirm.Visible = false;
            this.cb_confirm.Click += new System.EventHandler(this.cb_confirm_Click);
            // 
            // back_button
            // 
            this.back_button.Location = new System.Drawing.Point(209, 412);
            this.back_button.Name = "back_button";
            this.back_button.Size = new System.Drawing.Size(75, 32);
            this.back_button.TabIndex = 13;
            this.back_button.Text = "Back";
            this.back_button.UseVisualStyleBackColor = true;
            this.back_button.Visible = false;
            this.back_button.Click += new System.EventHandler(this.back_button_Click);
            // 
            // replay_button
            // 
            this.replay_button.Location = new System.Drawing.Point(61, 141);
            this.replay_button.Name = "replay_button";
            this.replay_button.Size = new System.Drawing.Size(100, 50);
            this.replay_button.TabIndex = 14;
            this.replay_button.Text = "Replay";
            this.replay_button.UseVisualStyleBackColor = true;
            this.replay_button.Visible = false;
            this.replay_button.Click += new System.EventHandler(this.replay_button_Click);
            // 
            // live_button
            // 
            this.live_button.Location = new System.Drawing.Point(64, 214);
            this.live_button.Name = "live_button";
            this.live_button.Size = new System.Drawing.Size(100, 50);
            this.live_button.TabIndex = 15;
            this.live_button.Text = "Live Gameplay";
            this.live_button.UseVisualStyleBackColor = true;
            this.live_button.Visible = false;
            this.live_button.Click += new System.EventHandler(this.live_button_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // go_button
            // 
            this.go_button.Location = new System.Drawing.Point(126, 297);
            this.go_button.Name = "go_button";
            this.go_button.Size = new System.Drawing.Size(150, 100);
            this.go_button.TabIndex = 16;
            this.go_button.Text = "Go";
            this.go_button.UseVisualStyleBackColor = true;
            this.go_button.Visible = false;
            this.go_button.Click += new System.EventHandler(this.go_button_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer_text
            // 
            this.timer_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.timer_text.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.timer_text.Location = new System.Drawing.Point(122, 115);
            this.timer_text.Name = "timer_text";
            this.timer_text.Size = new System.Drawing.Size(150, 150);
            this.timer_text.TabIndex = 17;
            this.timer_text.Text = "5";
            this.timer_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.timer_text.Visible = false;
            // 
            // hero_select_label
            // 
            this.hero_select_label.AutoSize = true;
            this.hero_select_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hero_select_label.ForeColor = System.Drawing.SystemColors.ControlText;
            this.hero_select_label.Location = new System.Drawing.Point(45, 75);
            this.hero_select_label.Name = "hero_select_label";
            this.hero_select_label.Size = new System.Drawing.Size(87, 13);
            this.hero_select_label.TabIndex = 18;
            this.hero_select_label.Text = "Select a hero:";
            this.hero_select_label.Visible = false;
            // 
            // hero_select_box
            // 
            this.hero_select_box.FormattingEnabled = true;
            this.hero_select_box.Location = new System.Drawing.Point(109, 51);
            this.hero_select_box.Name = "hero_select_box";
            this.hero_select_box.Size = new System.Drawing.Size(121, 21);
            this.hero_select_box.TabIndex = 19;
            this.hero_select_box.Visible = false;
            // 
            // hero_select_button
            // 
            this.hero_select_button.Location = new System.Drawing.Point(13, 412);
            this.hero_select_button.Name = "hero_select_button";
            this.hero_select_button.Size = new System.Drawing.Size(75, 32);
            this.hero_select_button.TabIndex = 20;
            this.hero_select_button.Text = "Confirm";
            this.hero_select_button.UseVisualStyleBackColor = true;
            this.hero_select_button.Visible = false;
            this.hero_select_button.Click += new System.EventHandler(this.hero_select_button_Click);
            // 
            // tick_timer
            // 
            this.tick_timer.Interval = 33;
            this.tick_timer.Tick += new System.EventHandler(this.tick_timer_Tick);
            // 
            // parsing_label
            // 
            this.parsing_label.AutoSize = true;
            this.parsing_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.parsing_label.Location = new System.Drawing.Point(60, 190);
            this.parsing_label.Name = "parsing_label";
            this.parsing_label.Size = new System.Drawing.Size(93, 25);
            this.parsing_label.TabIndex = 21;
            this.parsing_label.Text = "Parsing...";
            this.parsing_label.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 456);
            this.Controls.Add(this.parsing_label);
            this.Controls.Add(this.hero_select_button);
            this.Controls.Add(this.hero_select_box);
            this.Controls.Add(this.hero_select_label);
            this.Controls.Add(this.timer_text);
            this.Controls.Add(this.live_button);
            this.Controls.Add(this.replay_button);
            this.Controls.Add(this.back_button);
            this.Controls.Add(this.cb_confirm);
            this.Controls.Add(this.player_level);
            this.Controls.Add(this.go_button);
            this.Controls.Add(this.player_level_text);
            this.Controls.Add(this.checkbox_container);
            this.Controls.Add(this.almost_button);
            this.Controls.Add(this.learning_button);
            this.Controls.Add(this.novice_button);
            this.Controls.Add(this.title_label);
            this.Name = "Form1";
            this.Text = "Gaming Supervisor";
            this.checkbox_container.ResumeLayout(false);
            this.checkbox_container.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title_label;
        private System.Windows.Forms.Button novice_button;
        private System.Windows.Forms.Button learning_button;
        private System.Windows.Forms.Button almost_button;
        private System.Windows.Forms.CheckBox hs_checkbox;
        private System.Windows.Forms.CheckBox ih_checkbox;
        private System.Windows.Forms.CheckBox ln_checkbox;
        private System.Windows.Forms.CheckBox lh_checkbox;
        private System.Windows.Forms.CheckBox jg_checkbox;
        private System.Windows.Forms.CheckBox sfa_checkbox;
        private System.Windows.Forms.FlowLayoutPanel checkbox_container;
        private System.Windows.Forms.Label player_level_text;
        private System.Windows.Forms.Label player_level;
        private System.Windows.Forms.Button cb_confirm;
        private System.Windows.Forms.Button back_button;
        private System.Windows.Forms.Button replay_button;
        private System.Windows.Forms.Button live_button;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button go_button;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label timer_text;
        private System.Windows.Forms.Label hero_select_label;
        private System.Windows.Forms.ComboBox hero_select_box;
        private System.Windows.Forms.Button hero_select_button;
        private System.Windows.Forms.Timer tick_timer;
        private System.Windows.Forms.Label parsing_label;
    }
}

