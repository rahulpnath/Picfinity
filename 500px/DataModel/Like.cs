using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picfinity.DataModel
{
  public  class Like: BaseEntity
    {
        public string imageUrl { get; set; }

        private string text;
        public string Text { get { return text; } set { text = value; this.RaisePropertyChanged("Text"); } }

        public string Color { get; set; }

        private string count;
        public string Count { get { return count; } set { count = "+" + value; this.RaisePropertyChanged("Count"); } }

        public LikeType LikeType { get; set; }

    }

  public enum LikeType
  {
      Like,
      More,
      User
  }
}
