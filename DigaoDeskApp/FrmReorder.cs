﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DigaoDeskApp
{
    public partial class FrmReorder : Form
    {

        public static bool ReorderList<T>(List<T> list, string title)
        {
            FrmReorder f = new();
            f.Text = title;
            list.ForEach(x => f.list.Items.Add(x));
            if (f.ShowDialog() == DialogResult.OK)
            {
                list.Clear();
                foreach (T item in f.list.Items)
                {
                    list.Add(item);
                }
                return true;
            }
            return false;
        }

        public FrmReorder()
        {
            InitializeComponent();

            LoadLang();
        }

        private void LoadLang()
        {
            btnOK.Text = Vars.Lang.BtnOK;
            btnCancel.Text = Vars.Lang.BtnCancel;
        }

        private void FrmReorder_Load(object sender, EventArgs e)
        {
            list.AllowDrop = true;
        }

        private void list_MouseDown(object sender, MouseEventArgs e)
        {
            if (list.SelectedItem == null) return;
            list.DoDragDrop(list.SelectedItem, DragDropEffects.Move);
        }

        private void list_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void list_DragDrop(object sender, DragEventArgs e)
        {
            Point point = list.PointToClient(new Point(e.X, e.Y));
            int index = list.IndexFromPoint(point);
            if (index < 0) index = list.Items.Count-1;
            object data = list.SelectedItem;
            list.Items.Remove(data);
            list.Items.Insert(index, data);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

    }
}
