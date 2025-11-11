using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MPF.UI
{
    internal static class ItemHelper
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="TDependencyObject">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter.
        /// If not matching item can be found,
        /// a null parent is being returned.</returns>
        public static TDependencyObject? FindChild<TDependencyObject>(DependencyObject? parent, string childName) where TDependencyObject : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            TDependencyObject? foundChild = null;

            if (parent is ItemsControl itemsControl && itemsControl.Items != null)
            {
                int childrenCount = itemsControl.Items.Count;
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = itemsControl.Items[i] as DependencyObject;

                    // If the child is not of the request child type child
                    if (child is not TDependencyObject)
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<TDependencyObject>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null)
                            break;
                    }
                    else if (!string.IsNullOrEmpty(childName))
                    {
                        // If the child's name is set for search
                        if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                        {
                            // if the child's name is of the request name
                            foundChild = (TDependencyObject)child;
                            break;
                        }
                    }
                    else
                    {
                        // child element found.
                        foundChild = (TDependencyObject)child;
                        break;
                    }
                }
            }
            else if (parent is ContentControl contentControl && contentControl.Content != null)
            {
                var child = contentControl.Content as DependencyObject;

                // If the child is not of the request child type child
                if (child is not TDependencyObject)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<TDependencyObject>(child, childName);
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // If the child's name is set for search
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (TDependencyObject)child;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (TDependencyObject)child;
                }
            }
            else if (parent is Visual)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    // If the child is not of the request child type child
                    if (child is not TDependencyObject)
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<TDependencyObject>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null)
                            break;
                    }
                    else if (!string.IsNullOrEmpty(childName))
                    {
                        // If the child's name is set for search
                        if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                        {
                            // if the child's name is of the request name
                            foundChild = (TDependencyObject)child;
                            break;
                        }
                    }
                    else
                    {
                        // child element found.
                        foundChild = (TDependencyObject)child;
                        break;
                    }
                }
            }

            return foundChild;
        }
    }
}
