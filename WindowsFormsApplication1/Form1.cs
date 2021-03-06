﻿//Paste V1.62

using System;
using System.Windows.Forms;
using Hotkeys;
using System.IO;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        String backup_clipboard,temp_clipboard;
        //##########################################FORM LOAD##############################################################
        private void Form1_Load(object sender, EventArgs e)
        {

            if (ghk.Register())
                label4.Text = "Hotkey CTRL+1 registered.";
            else
                label4.Text = "Hotkey CTRL+1 failed to register";

            //this.Resize += new System.EventHandler(this.Form1_SizeChanged); // always hide to tray when minimized
            notifyIcon1.Click += NotifyIcon1_Click;
  
            //tray icon context menu
            MenuItem[] menuList = new MenuItem[] { new MenuItem("Open") };
            ContextMenu clickMenu = new ContextMenu(menuList);
            notifyIcon1.ContextMenu = clickMenu;

            //act according to some settings in config
            read_config();
        }
        //################################################################################################################
        //------------------------------------------------------------------------------------------------------

        //read config from file
        private void read_config()
        {
            String line;
            try
            {
                StreamReader sr = new StreamReader("C:\\Paste\\paste_config.txt");      //open file
                line = sr.ReadLine();

                while (line != null)                             //go via lines
                {
                    if (line.Contains("StartInTray=yes")) //necessary first, because ShowInTaskbar=false breaks the app otherwise
                    {
                        ShowInTaskbar = false;
                        TrayButton_Click(null, null);

                    }
                    if (line.Contains("EnableSecondClipboard=yes"))
                    {
                        checkBox1.Checked = true;       //Checkbox for Second Clipboard
                        manage_backup_clipboard();  
                    }
                    if (line.Contains("PasteHotKey=CTRL+1"))
                        button2_Click(null, null);
                    if (line.Contains("PasteHotKey=CTRL+\""))
                        button4_Click(null, null);
                    if (line.Contains("PasteHotKey=CTRL+5"))
                        button3_Click(null, null);
                    if (line.Contains("PasteHotKey=CTRL+space"))
                        button5_Click(null, null);
                    
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e) { Console.WriteLine("Exception: " + e.Message); }
            finally { Console.WriteLine("Done."); }
        }

        //main pasting method
        
        private void paste_text_from_clipboard(String by_hotkey, String text)
        {
            if (by_hotkey == "by_button") SendKeys.Send("%{TAB}");              //alt tab if launched by button
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.4));       //pause 0.4sec, because some chars have been cut

            if (text != null)
                for (int i = 0; i < text.Length; i++)          //types char by char, while treating special chars in other way
                {
                    char C = text[i];
                    float DELAY = float.Parse(textBox1.Text);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(DELAY / 1000));  //DELAY defined to pause between chars  
                    if (C == '(') { SendKeys.Send("{(}"); continue; }
                    if (C == ')') { SendKeys.Send("{)}"); continue; }
                    if (C == '+') { SendKeys.Send("{+}"); continue; }
                    if (C == '^') { SendKeys.Send("{^}"); continue; }
                    if (C == '%') { SendKeys.Send("{%}"); continue; }
                    if (C == '~') { SendKeys.Send("{~}"); continue; }
                    if (C == '{') { SendKeys.Send("{{}"); continue; }
                    if (C == '}') { SendKeys.Send("{}}"); continue; }

                    SendKeys.Send(C.ToString());
                }
        }


        private void clipboard2_add() //first backups clipboard to temp, then uses ctrl+c to capture data, then reverts previous clipboard text
        {
            temp_clipboard = Clipboard.GetText(TextDataFormat.Text);
            SendKeys.Send("^c");
            backup_clipboard = Clipboard.GetText(TextDataFormat.Text);
            Clipboard.SetText(temp_clipboard);
        }

        private void clipboard2_paste()
        {
            paste_text_from_clipboard("by_hotkey", backup_clipboard);
        }


        //PART to take the action on the hotkey press
        private void HandleHotkey()
        {
            paste_text_from_clipboard("by_hotkey", Clipboard.GetText(TextDataFormat.Text));
        }
        //....or action for button press:
        private void button1_Click(object sender, EventArgs e)
        {
            paste_text_from_clipboard("by_button", Clipboard.GetText(TextDataFormat.Text));
        }

        //------------------------------------------------------------------------------------------------------
        //PART for HOTKEY support:
        private Hotkeys.GlobalHotkey ghk, ghk2, ghk3;

        public Form1()
        {
            InitializeComponent();
            ghk = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D1, this);   //Use CTRL and digit 1 for paste from clipboard
            ghk2 = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D2, this);   //Use CTRL and digit 2 for move to backup clipboard
            ghk3 = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D3, this);   //Use CTRL and digit 3 for paste from backup clipboard
           
        }

        private Keys GetKey(IntPtr LParam)
        {
            return (Keys)((LParam.ToInt32()) >> 16);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Hotkeys.Constants.WM_HOTKEY_MSG_ID)
            {
                switch (GetKey(m.LParam))
                {
                    case (Keys.D1):     //paste from clipboard
                        HandleHotkey();
                        break;
                    case (Keys.D5):     //paste from clipboard
                        HandleHotkey();
                        break;
                    case (Keys.OemQuotes):     //paste from clipboard
                        HandleHotkey();
                        break;
                    case (Keys.Space):     //paste from clipboard
                        HandleHotkey();
                        break;

                    case (Keys.D2):     //modify for use backup clipboard
                        clipboard2_add();
                        break;

                    case (Keys.D3):     //modify to use to paste from backup clipboard
                        clipboard2_paste();
                        break;
                }
            }

            base.WndProc(ref m);
        }

        //------------------------------------------------------------------------------------------------------
        //System Tray methods

        //Show Form again
        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
            this.Opacity = 1;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)       //if the form is minimized  hide it from the task bar   
        {                                                               //and show the system tray icon (represented by the NotifyIcon control) 
            if (this.WindowState == FormWindowState.Minimized)           //not used at this time
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void TrayButton_Click(object sender, EventArgs e)
        {
            //hide to system tray button
            Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000);
            this.Opacity = 0.0f;
        }

        //------------------------------------------------------------------------------------------------------
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ghk.Unregiser() && !ghk2.Unregiser() && !ghk3.Unregiser())
                MessageBox.Show("Hotkey failed to unregister!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ghk.Unregiser();                                                   //unregister before registering again
            ghk = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D1, this);   //Use CTRL and digit 1
            if (ghk.Register())
                label4.Text = "Hotkey CTRL+1 registered.";
            else
                label4.Text = "Hotkey CTRL+1 failed to register";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ghk.Unregiser();                                                //unregister before registering again
            ghk = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D5, this);   //Use CTRL and digit 5
            if (ghk.Register())
                label4.Text = "Hotkey CTRL+5 registered.";
            else
                label4.Text = "Hotkey CTRL+5 failed to register";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ghk.Unregiser();                                                //unregister before registering again
            ghk = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.OemQuotes, this);   //Use CTRL and quotes
            if (ghk.Register())
                label4.Text = "Hotkey CTRL+\" registered.";
            else
                label4.Text = "Hotkey CTRL+\" failed to register";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ghk.Unregiser();                                                //unregister before registering again
            ghk = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.Space, this);   //Use CTRL and space
            if (ghk.Register())
                label4.Text = "Hotkey CTRL+space registered.";
            else
                label4.Text = "Hotkey CTRL+space failed to register";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/jarekj9/paste");
        }

      

        //Second clipboard checkbox
        private void manage_backup_clipboard()
        {
            ghk2 = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D2, this);
            ghk3 = new Hotkeys.GlobalHotkey(Constants.CTRL, Keys.D3, this);

            if (checkBox1.Checked)
                if (ghk2.Register() && ghk3.Register()) SecClipLabel.Text = "CTRL+2 and CTRL+3 registered";
            if (!checkBox1.Checked)
                if (ghk2.Unregiser() && ghk3.Unregiser()) SecClipLabel.Text = "CTRL+2 and CTRL+3 not registered";
        }


        private void checkBox1_Clicked(object sender, EventArgs e)
        {
            manage_backup_clipboard();
        }

        private void button6_Click(object sender, EventArgs e)          //help button
        {
            System.Windows.Forms.MessageBox.Show(
            "- Use at your own risk\n\n" +
            "- Application registers hotkey(ctrl + 1 or other) - only If this hotkey has not been already taken by other application.\n\n" +
            "- Pressing hotkey or pressing 'PASTE' button will take text from your clipboard and simulate keyboard keypresses to type this text. Effect is the same like typing on keyboard.\n\n" +
            "- With 'Second clipboard' enabled, use: Ctrl+2 to copy, Ctrl+3 to paste\n\n" +
            "- Uninstall old version from control panel before installing new.\n\n" +
            "- You may change default settings if you put paste_config.txt file in folder C:\\Paste\\\n\n" +
            "- Attention1: Sometimes some letter may be lost due to system / network performance.\n\n" +
            "- Attention2: Sometimes the destination, where you type, may recognize certain quick keypresses as its own shortcuts / hotkeys. (for example : better disable 'autocomplete' in notepad++ if you paste into notepad++ ).\n\n\n\n" +
            "Version information:\n" +
            "Version 1.62 - restored option to hide in Tray with config file (but there is a bug in Windows7)\n" +
            "Version 1.61 - changed behaviour of backup clipboard\n" +
            "Version 1.6 - added Second Clipboard and config file\n\n" +
            "In case of questions contact me at jaroslaw.jankun@gmail.com\n\n" +
            "Copyright (C) 2018\n\n" +
            "This program is free software: you can redistribute it and/or modify\n" +
            "it under the terms of the GNU General Public License as published by\n" +
            "the Free Software Foundation, either version 3 of the License, or\n" +
            "(at your option) any later version.\n\n" +
            "This program is distributed in the hope that it will be useful,\n" +
            "but WITHOUT ANY WARRANTY; without even the implied warranty of\n" +
            "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n" +
            "GNU General Public License for more details.\n\n" +
            "You should have received a copy of the GNU General Public License\n" +
            "along with this program.  If not, see http://www.gnu.org/licenses/"
            );
        }


    }
}