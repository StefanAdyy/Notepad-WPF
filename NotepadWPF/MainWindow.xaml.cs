using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using NotepadWPF.misc;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualBasic;
using System.Text;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;

namespace NotepadWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer = new DispatcherTimer();
        DateTime startTime;
        string filePath;
        string savedText;
        public MainWindow()
        {
            InitializeComponent();
            startTime = DateTime.Now;
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            filePath = String.Empty;
            savedText = textBox.Text;
            LoadDirectories();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime date;
            date = DateTime.Now;
            TxtClock.Text = date.ToLongTimeString() + "   " + date.ToLongDateString();
            var elapsedTime = (int)date.Subtract(startTime).TotalMinutes;
            TxtTimer.Text = $"{elapsedTime.ToString()} {(elapsedTime == 1 ? "minute" : "minutes")} elapsed";
            TxtBattery.Text = Utils.GetBatteryPercentage();
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            if (TimeCard.Visibility == Visibility.Hidden)
            {
                TimeCard.Visibility = Visibility.Visible;
                BatteryCard.Visibility = Visibility.Visible;
                ElapsedTimeCard.Visibility = Visibility.Visible;
            }
            else
            {
                TimeCard.Visibility = Visibility.Hidden;
                BatteryCard.Visibility = Visibility.Hidden;
                ElapsedTimeCard.Visibility = Visibility.Hidden;
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string wordToFind = Interaction.InputBox("Type the word you wand to find.", "Find", "Word to find", 700, 200);

            if (wordToFind != string.Empty)
            {
                int wordIndex = textBox.Text.IndexOf(wordToFind);

                while (wordIndex >= 0 && wordIndex < textBox.Text.Length - 1)
                {
                    textBox.Select(wordIndex, wordToFind.Length);
                    await Task.Delay(1000);

                    MessageBoxResult result = CreateYesNoMessageBox("Find next ", "Find next occurrence");
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            wordIndex = textBox.Text.IndexOf(wordToFind, wordIndex + 1);
                            break;
                        case MessageBoxResult.No:
                            wordIndex = -1;
                            break;
                        default:
                            wordIndex = -1;
                            break;
                    }
                }
            }
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            string wordToFind = Interaction.InputBox("Type the word you wand to find.", "Find", "Word to find", 700, 200);

            if (wordToFind != string.Empty)
            {
                int wordIndex = textBox.Text.IndexOf(wordToFind);

                if (wordIndex >= 0)
                {
                    string wordToReplaceWith = Interaction.InputBox("Type the new word.", "Substitute word", "Word to find", 700, 200);

                    if (wordToReplaceWith != string.Empty)
                    {
                        ReplaceWord(wordToFind, wordToReplaceWith);
                    }
                }
            }
        }

        private void ReplaceWord(string oldValue, string newValue)
        {
            string replacedValues = textBox.Text;
            replacedValues = replacedValues.Replace(oldValue, newValue);
            textBox.Text = replacedValues;
        }
        private void WrapBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeWordWrap();
        }
        private void WrapIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeWordWrap();
        }
        private void ChangeWordWrap()
        {
            if (textBox.TextWrapping == TextWrapping.Wrap)
            {
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                textBox.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                textBox.TextWrapping = TextWrapping.Wrap;
            }
        }
        private void FontDecreaseIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeFontSize(false);
        }
        private void FontIncreaseIcon_Click(object sender, RoutedEventArgs e)
        {
            ChangeFontSize(true);
        }

        private void ChangeFontSize(bool increase)
        {
            if (increase)
            {
                if (textBox.FontSize <= 60)
                {
                    textBox.FontSize += 2;
                }
            }
            else
            {
                if (textBox.FontSize >= 6)
                {
                    textBox.FontSize -= 2;
                }
            }
        }

        private string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange.Text;
        }
        private System.Windows.MessageBoxResult CreateYesNoMessageBox(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }
        //---------------------------------------------------------------------------------------------------SAVE--------------------------------------------------------------------------------------------------------
        private void SaveAndExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (savedText != textBox.Text)
            {
                var result = CreateYesNoMessageBox("Save file before exiting?", "Save file");

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        break;

                    case MessageBoxResult.Yes:
                        SaveFile();
                        this.Close();
                        break;

                    case MessageBoxResult.No:
                        this.Close();
                        break;
                }
            }
            else
            {
                this.Close();
            }
        }
        private void SaveIcon_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }
        private void SaveFile()
        {
            if (filePath == String.Empty)
            {
                SaveFileAs();
            }
            else
            {
                if (savedText != textBox.Text)
                {
                    savedText = textBox.Text;
                    StreamWriter sw = new StreamWriter(File.OpenWrite(filePath));
                    sw.Write(savedText);
                    sw.Dispose();
                }
            }
        }
        private void SaveFileAs()
        {
            SaveFileDialog save = new SaveFileDialog()
            {
                Title = "Save your File",
                Filter = "Text Document (*.txt) | .txt",
                FileName = ""
            };

            if (save.ShowDialog() == true)
            {
                savedText = textBox.Text;
                StreamWriter sw = new StreamWriter(File.Create(save.FileName));
                filePath = save.FileName;
                sw.Write(savedText);
                sw.Dispose();
            }

            FileName.Text = "Filename: " + System.IO.Path.GetFileNameWithoutExtension(save.FileName);
        }
        //---------------------------------------------------------------------------------------------------SAVE--------------------------------------------------------------------------------------------------------
        private void MoreIcon_Click(object sender, RoutedEventArgs e)
        {
            if (SaveIcon.Visibility == Visibility.Visible)
            {
                SaveIcon.Visibility = Visibility.Hidden;
                OpenIcon.Visibility = Visibility.Hidden;
                SearchIcon.Visibility = Visibility.Hidden;
                WrapIcon.Visibility = Visibility.Hidden;
                FontIncreaseIcon.Visibility = Visibility.Hidden;
                FontDecreaseIcon.Visibility = Visibility.Hidden;
                ChooseFont.Visibility = Visibility.Hidden;
                ChooseFontSize.Visibility = Visibility.Hidden;
                Tool.Visibility = Visibility.Hidden;
                EditRights.Visibility = Visibility.Hidden;
                LineControl.Visibility = Visibility.Hidden;
                Help.Visibility = Visibility.Hidden;
                Theme.Visibility = Visibility.Hidden;
            }
            else
            {
                SaveIcon.Visibility = Visibility.Visible;
                OpenIcon.Visibility = Visibility.Visible;
                SearchIcon.Visibility = Visibility.Visible;
                WrapIcon.Visibility = Visibility.Visible;
                FontIncreaseIcon.Visibility = Visibility.Visible;
                FontDecreaseIcon.Visibility = Visibility.Visible;
                ChooseFont.Visibility = Visibility.Visible;
                ChooseFontSize.Visibility = Visibility.Visible;
                Tool.Visibility = Visibility.Visible;
                EditRights.Visibility = Visibility.Visible;
                LineControl.Visibility = Visibility.Visible;
                Help.Visibility = Visibility.Visible;
                Theme.Visibility = Visibility.Visible;
            }
        }

        private void OpenIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            if (savedText != textBox.Text)
            {
                var result = CreateYesNoMessageBox("Save this file before opening another one?", "Save file");

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        break;

                    case MessageBoxResult.Yes:
                        SaveFile();
                        OpenFileDialog();
                        break;

                    case MessageBoxResult.No:
                        OpenFileDialog();
                        break;
                }
            }
            else
            {
                OpenFileDialog();
            }
        }

        private void OpenFileFromPath(string path)
        {
            if (path != null)
            {
                if (savedText != textBox.Text)
                {
                    var result = CreateYesNoMessageBox("Save this file before opening another one?", "Save file");

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                            break;

                        case MessageBoxResult.Yes:
                            SaveFile();
                            break;

                        case MessageBoxResult.No:
                            break;
                    }
                }
                else
                {
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    textBox.Text = sr.ReadToEnd();
                    sr.Dispose();
                    filePath = path;
                    savedText = textBox.Text;
                    FileName.Text = "Filename: " + System.IO.Path.GetFileNameWithoutExtension(filePath);
                }
            }

        }
        private void OpenFileDialog()
        {
            OpenFileDialog open = new OpenFileDialog()
            {
                Title = "Open your File",
                FileName = ""
            };

            if (open.ShowDialog() == true)
            {
                StreamReader sr = new StreamReader(File.OpenRead(open.FileName));
                textBox.Text = sr.ReadToEnd();
                sr.Dispose();
                filePath = open.FileName;
                savedText = textBox.Text;
                FileName.Text = "Filename: " + System.IO.Path.GetFileNameWithoutExtension(open.FileName);
            }
        }
        private void FontBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseFont.Visibility != Visibility.Visible)
            {
                ChooseFont.Visibility = Visibility.Visible;
                ChooseFont.IsDropDownOpen = true;
            }
            else
            {
                ChooseFont.Visibility = Visibility.Hidden;
                ChooseFont.IsDropDownOpen = false;
            }
        }

        private void FontSizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ChooseFontSize.Visibility != Visibility.Visible)
            {
                ChooseFontSize.Visibility = Visibility.Visible;
                ChooseFontSize.IsDropDownOpen = true;
            }
            else
            {
                ChooseFontSize.Visibility = Visibility.Hidden;
                ChooseFontSize.IsDropDownOpen = false;
            }
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (savedText != textBox.Text)
            {
                var result = CreateYesNoMessageBox("Save this file before creating a new one?", "Save file");

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        break;

                    case MessageBoxResult.Yes:
                        SaveFile();
                        CreateNewFile();
                        break;

                    case MessageBoxResult.No:
                        CreateNewFile();
                        break;
                }
            }
            else
            {
                CreateNewFile();
            }
        }

        private void CreateNewFile()
        {
            startTime = DateTime.Now;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            filePath = String.Empty;
            textBox.Clear();
            FileName.Text = "Filename: ";
            savedText = textBox.Text;
        }

        private void HighLight_Click(object sender, RoutedEventArgs e)
        {
            //richTextBox.selec
            //richTextBox.Selection.Select(0, 10);
        }

        private void FindBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAsync(new CancellationToken());
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = CreateYesNoMessageBox("This text editor (Fake Notepad) was made by MORARIU ADRIAN STEFAN from 10L302. Would you like to see his school profile?", "Developer");

            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start("https://elearning.unitbv.ro/user/profile.php?id=20467");
            }
        }

        private void Tool_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (Tool.SelectedIndex)
            {
                case 0:
                    Clipboard.SetText(textBox.SelectedText);
                    break;

                case 1:
                    Clipboard.SetText(textBox.SelectedText);
                    textBox.SelectedText = "";
                    break;

                case 2:
                    textBox.Text = textBox.Text.Insert(textBox.SelectionStart, Clipboard.GetText());
                    break;

                case 3:
                    if (textBox != null)
                    {
                        textBox.SelectedText = textBox.SelectedText.ToLower();
                    }
                    break;

                case 4:
                    if (textBox != null)
                    {
                        textBox.SelectedText = textBox.SelectedText.ToUpper();
                    }
                    break;

                default:
                    break;
            }
        }

        private void ChooseFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)ChooseFontSize.SelectedItem;
            string selectedText = cbi.Content.ToString();
            if (textBox != null && selectedText != null)
            {
                textBox.FontSize = Double.Parse(selectedText);
            }
        }

        private void ChooseFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textBox != null)
            {
                switch (ChooseFont.SelectedIndex)
                {
                    case 0:
                        textBox.FontFamily = new FontFamily("Tahoma");
                        break;

                    case 1:
                        textBox.FontFamily = new FontFamily("Futura");
                        break;

                    case 2:
                        textBox.FontFamily = new FontFamily("Verdana");
                        break;

                    case 3:
                        textBox.FontFamily = new FontFamily("Consolas");
                        break;

                    case 4:
                        textBox.FontFamily = new FontFamily("Calibri light");
                        break;

                    case 5:
                        textBox.FontFamily = new FontFamily("Times New Roman");
                        break;

                    default:
                        break;
                }
            }
        }

        private void FolderTree_Click(object sender, RoutedEventArgs e)
        {
            ChangeTreeIcon();
        }

        private void ChangeTreeIcon()
        {
            if (FolderTree.Header == "\xE973")
            {
                FolderTree.Header = "\xE974";
                treeView.Visibility = Visibility.Collapsed;
            }
            else
            {
                FolderTree.Header = "\xE973";
                treeView.Visibility = Visibility.Visible;
            }
        }

        private void EditRights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (textBox != null)
            {
                switch (EditRights.SelectedIndex)
                {
                    case 0:
                        textBox.IsReadOnly = false;
                        break;

                    case 1:
                        textBox.IsReadOnly = true;
                        break;

                    default:
                        break;
                }
            }
        }

        private void LineControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (LineControl.SelectedIndex)
            {
                case 0:
                    GoToLine();
                    break;

                case 1:
                    RemoveEmptyLines();
                    break;

                default:
                    break;
            }

        }

        private void RemoveEmptyLines()
        {
            textBox.Text = Regex.Replace(textBox.Text, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline);
        }

        private void GoToLine()
        {
            if (textBox != null)
            {
                switch (LineControl.SelectedIndex)
                {
                    case 0:
                        string lineNumber = Interaction.InputBox("Line number.", "Line number to go to", "", 700, 200);
                        if (lineNumber != null)
                        {
                            int index;
                            if (int.TryParse(lineNumber, out index) && index > 0 && index <= textBox.LineCount)
                            {
                                textBox.SelectionStart = textBox.GetCharacterIndexFromLineIndex(index - 1);
                                textBox.SelectionLength = textBox.GetLineLength(index - 1);
                                textBox.CaretIndex = textBox.SelectionStart;
                                textBox.ScrollToLine(index - 1);
                                textBox.Focus();
                            }
                            else
                            {
                                MessageBox.Show("You must provide a proper line number!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("You must provide a number!");
                        }
                        break;

                    case 1:
                        this.Close();
                        break;

                    default:
                        break;
                }
            }
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Theme.SelectedIndex)
            {
                case 0:
                    ChangeColor(System.Windows.Media.Brushes.Gainsboro, System.Windows.Media.Brushes.Black, System.Windows.Media.Brushes.MediumPurple, System.Windows.Media.Brushes.Black);
                    break;

                case 1:
                    ChangeColor(new SolidColorBrush(Color.FromRgb(30, 30, 30)), new SolidColorBrush(Color.FromRgb(160, 192, 207)), new SolidColorBrush(Color.FromRgb(45, 45, 48)), new SolidColorBrush(Color.FromRgb(47, 95, 140)));
                    break;

                case 2:
                    ChangeColor(new SolidColorBrush(Color.FromRgb(0, 0, 0)), new SolidColorBrush(Color.FromRgb(0, 255, 0)), new SolidColorBrush(Color.FromRgb(0, 0, 0)), new SolidColorBrush(Color.FromRgb(0, 255, 0)));
                    break;

                case 3:
                    ChangeColor(new SolidColorBrush(Color.FromRgb(250, 117, 170)), new SolidColorBrush(Color.FromRgb(255, 255, 255)), new SolidColorBrush(Color.FromRgb(246, 19, 110)), new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                    break;

                default:
                    break;
            }
        }

        private void ChangeColor(Brush background, Brush txtColor, Brush headerFooterColor, Brush hfTextColor)
        {
            this.Background = background;
            TopBar.Background = headerFooterColor;

            if (BottomBar != null && FileName != null && Help != null && treeView != null)
            {
                BottomBar.Background = headerFooterColor;
                FileName.Foreground = hfTextColor;
                Help.Foreground = hfTextColor;
                treeView.Foreground = hfTextColor;
            }

            FolderTree.Foreground = hfTextColor;
            FileBtn.Foreground = hfTextColor;
            OptionsBtn.Foreground = hfTextColor;
            Stats.Foreground = hfTextColor;
            MoreIcon.Foreground = hfTextColor;
            SaveIcon.Foreground = hfTextColor;
            OpenIcon.Foreground = hfTextColor;
            SearchIcon.Foreground = hfTextColor;
            WrapIcon.Foreground = hfTextColor;
            FontDecreaseIcon.Foreground = hfTextColor;
            FontSizeItem0.Foreground = hfTextColor;
            FontSizeItem1.Foreground = hfTextColor;
            FontSizeItem2.Foreground = hfTextColor;
            FontSizeItem3.Foreground = hfTextColor;
            FontSizeItem4.Foreground = hfTextColor;
            FontSizeItem5.Foreground = hfTextColor;
            FontSizeItem6.Foreground = hfTextColor;
            FontSizeItem7.Foreground = hfTextColor;
            FontItem0.Foreground = hfTextColor;
            FontItem1.Foreground = hfTextColor;
            FontItem2.Foreground = hfTextColor;
            FontItem3.Foreground = hfTextColor;
            FontItem4.Foreground = hfTextColor;
            FontItem5.Foreground = hfTextColor;
            FontIncreaseIcon.Foreground = hfTextColor;
            ChooseFontSize.Foreground = hfTextColor;
            ChooseFont.Foreground = hfTextColor;
            Tool.Foreground = hfTextColor;
            ToolItem0.Foreground = hfTextColor;
            ToolItem1.Foreground = hfTextColor;
            ToolItem2.Foreground = hfTextColor;
            ToolItem3.Foreground = hfTextColor;
            ToolItem4.Foreground = hfTextColor;
            EditRights.Foreground = hfTextColor;
            EditItem0.Foreground = hfTextColor;
            EditItem1.Foreground = hfTextColor;
            LineControl.Foreground = hfTextColor;
            LineControlItem0.Foreground = hfTextColor;
            LineControlItem1.Foreground = hfTextColor;
            Theme.Foreground = hfTextColor;
            ThemeItem0.Foreground = hfTextColor;
            ThemeItem1.Foreground = hfTextColor;
            ThemeItem2.Foreground = hfTextColor;
            ThemeItem3.Foreground = hfTextColor;

            if (FileName != null && Help != null && textBox != null && treeView != null)
            {
                FileName.Background = headerFooterColor;
                Help.Background = headerFooterColor;
                textBox.Foreground = txtColor;
                treeView.Background = headerFooterColor;
            }
            FolderTree.Background = headerFooterColor;
            FileBtn.Background = headerFooterColor;
            OptionsBtn.Background = headerFooterColor;
            Stats.Background = headerFooterColor;
            ChooseFontSize.Background = headerFooterColor;
            FontSizeItem0.Background = headerFooterColor;
            FontSizeItem1.Background = headerFooterColor;
            FontSizeItem2.Background = headerFooterColor;
            FontSizeItem3.Background = headerFooterColor;
            FontSizeItem4.Background = headerFooterColor;
            FontSizeItem5.Background = headerFooterColor;
            FontSizeItem6.Background = headerFooterColor;
            FontSizeItem7.Background = headerFooterColor;
            ChooseFont.Background = headerFooterColor;
            FontItem0.Background = headerFooterColor;
            FontItem1.Background = headerFooterColor;
            FontItem2.Background = headerFooterColor;
            FontItem3.Background = headerFooterColor;
            FontItem4.Background = headerFooterColor;
            FontItem5.Background = headerFooterColor;
            Tool.Background = headerFooterColor;
            ToolItem0.Background = headerFooterColor;
            ToolItem1.Background = headerFooterColor;
            ToolItem2.Background = headerFooterColor;
            ToolItem3.Background = headerFooterColor;
            ToolItem4.Background = headerFooterColor;
            EditRights.Background = headerFooterColor;
            EditItem0.Background = headerFooterColor;
            EditItem1.Background = headerFooterColor;
            LineControl.Background = headerFooterColor;
            LineControlItem0.Background = headerFooterColor;
            LineControlItem1.Background = headerFooterColor;
            Theme.Background = headerFooterColor;
            ThemeItem0.Background = headerFooterColor;
            ThemeItem1.Background = headerFooterColor;
            ThemeItem2.Background = headerFooterColor;
            ThemeItem3.Background = headerFooterColor;
        }

        public void LoadDirectories()
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                treeView.Items.Add(GetItem(drive));
            }
        }
        private TreeViewItem GetItem(DriveInfo drive)
        {
            var item = new TreeViewItem
            {
                Header = drive.Name,
                DataContext = drive,
                Tag = drive
            };
            this.AddChild(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            return item;
        }
        public class ChildTreeViewItem : TreeViewItem
        {
            public ChildTreeViewItem()
                : base()
            {
                base.Header = "Child";
                base.Tag = "Child";
            }
        }
        private TreeViewItem GetItem(DirectoryInfo directory)
        {
            var item = new TreeViewItem
            {
                Header = directory.Name,
                DataContext = directory,
                Tag = directory
            };
            this.AddChild(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            return item;
        }
        private TreeViewItem GetItem(FileInfo file)
        {
            var item = new TreeViewItem
            {
                Header = file.Name,
                DataContext = file,
                Tag = file.FullName
            };

            item.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(OpenFileEventHandler);

            return item;
        }

        void OpenFileEventHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (TreeViewItem)sender;
            OpenFileFromPath(item.Tag.ToString());
            ChangeTreeIcon();
            treeView.Visibility = Visibility.Collapsed;
        }

        private void AddChild(TreeViewItem item)
        {
            item.Items.Add(new ChildTreeViewItem());
        }

        private bool HasChild(TreeViewItem item)
        {
            return item.HasItems && (item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is ChildTreeViewItem).Count > 0);
        }

        private void RemoveChild(TreeViewItem item)
        {
            var dummies = item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is ChildTreeViewItem);
            foreach (var dummy in dummies)
            {
                item.Items.Remove(dummy);
            }
        }
        private void ExploreDirectories(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(directory));
                }
            }
        }

        private void ExploreFiles(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var file in directoryInfo.GetFiles())
            {
                var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(file));
                }
            }
        }
        void item_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (this.HasChild(item))
            {
                this.Cursor = Cursors.Wait;
                this.RemoveChild(item);
                this.ExploreDirectories(item);
                this.ExploreFiles(item);
                this.Cursor = Cursors.Arrow;
            }
        }
    }
}
