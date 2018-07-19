using System.Collections.Generic;
using System.Text;

namespace Axion.Processing.Tokens {

   /// <summary>
   ///   Standard <see cref="LinkedList{T}" /> implementation
   ///   with overridden <see cref="ToString"/> method to simplify debugging.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class Chain<T> : LinkedList<T> {

      public override string ToString() {
         var sb = new StringBuilder();

         LinkedListNode<T> node = First;
         var i = 0;

         while (node != null) {
            sb.Append(i + ": " + node.Value + "\n");
            i++;
            node = node.Next;
         }

         return sb.ToString();
      }
   }
}