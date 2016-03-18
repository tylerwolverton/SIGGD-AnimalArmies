/**
 * @file
 * @author PURDUE ACM, SIGGD <siggd.purdue@gmail.com>
 * @version 1.0
 * 
 * @section LICENSE
 * 
 * @section DESCRIPTION
 * 
 * Implements a pairing heap, the simplest heap I've come across.
 * It also allows elements already in the heap to be decreased, which is
 * essential for any implementation of Dijkstra's algorithm.
 * The basic idea is to just have each node keep a straight list of the nodes
 * below it, then when the top is removed, just combine elements of the list
 * in pairs from left to right, then combine them linearly from right to left.
 * Some fancy ACM paper proves that this can be done in logarithmic amortized
 * time, which is nice. Only what was directly applicable for Dijkstra's
 * algorithm was implemented.
 */

using System;
using System.Collections.Generic;

/// brief class desc here
/**
* classdeschere
*/
public class HeapNode<T>
{

    readonly T node;    ///< The contents of this particular node
    HeapNode<T> next;   ///< The next node in this sibling list
    HeapNode<T> prev;   ///< The prev node in this sibling list
    HeapNode<T> child;  ///< The youngest of this node's children
    HeapNode<T> par;    ///< This node's parent

    /**
    * desc here
    * 
    * @param paramsdeschere
    * 
    * @return returndeschere
    */
    public HeapNode(T node)
    {

        this.node = node;
        next = prev = par = child = null;
    }

    /**
    * Remove this node from any child list it might be in, cleanly.
    */
    public void extract()
    {

        if (prev != null)
        {
            prev.next = next;
        }
        else if (par != null && par.child == this)
        {
            par.child = next;
        }

        if (next != null) next.prev = prev;
    }

    /**
    * Adds a node to this node's children
    * 
    * @param newChild deschere
    */
    public void addChild(HeapNode<T> newChild)
    {

        newChild.next = child;
        newChild.prev = null;
        newChild.par = this;

        if (child != null)
        {
            child.prev = newChild;
        }

        child = newChild;
    }

    /**
    * desc here
    * 
    * @return returndeschere
    */
    public T getNode() { return node; }

    /**
    * desc here
    * 
    * @return returndeschere
    */
    public HeapNode<T> getNext() { return next; }

    /**
    * desc here
    * 
    * @return returndeschere
    */
    public HeapNode<T> getPrev() { return prev; }

    /**
    * desc here
    * 
    * @return returndeschere
    */
    public HeapNode<T> getChild() { return child; }

    /**
    * desc here
    * 
    * @return returndeschere
    */
    public HeapNode<T> getPar() { return par; }
}

/// brief class desc here
/**
* This class represents the heap as a whole
*/
public class Heap<T>
{

    public delegate int Comparator(T a, T b); ///<

    private HeapNode<T> top;   // The smallest node in the heap
    private int size;          // The size of the heap
    private Comparator compare;

    /**
    * Merge two subtrees together, returning the one that came out on top.
    * For a pairing heap, merging consists of adding the larger node
    * to the sub list of the smaller.
    * 
    * @param paramsdeschere
    * 
    * @return returndeschere
    */
    private HeapNode<T> merge(HeapNode<T> a, HeapNode<T> b)
    {

        if (compare(a.getNode(), b.getNode()) <= 0) // a <= b (swap a and b)
        {

            HeapNode<T> tmp = a;
            a = b; b = tmp;
        }


        // Here we assume that a > b, therefore a is added to b's children

        // Remove a from any child list it might be in.
        a.extract();

        // Link a into b's child list
        b.addChild(a);

        return b;
    }

    /**
    * desc here
    *
    * @param paramsdeschere
    *
    * @return returndeschere
    */
    public Heap(Comparator compare)
    {

        this.compare = compare;
        top = null;
        size = 0;
    }

    
    /**
    * Insert an element into the heap, returning a pointer to the node
    * that contains it (to allow for later decreaseKey).
    * 
    * @param paramsdeschere
    * 
    * @return returndeschere
    */
    public HeapNode<T> insert(T element)
    {
        HeapNode<T> node = new HeapNode<T>(element);

        if (isEmpty())// List is currently empty
        {
            top = node;
        }
        else if (merge(top, node) == node)
        {  // Node has become new top
            top = node;
        }

        size++;

        return node;
    }

    /**
    * Delete the mininum node, and return it. This is where the log n
    * magic happens. Pairs the sub-list together, then sweeps up the pairs
    * into one tree again.
    * 
    * @return returndeschere
    */
    public T deleteMin()
    {

        if (isEmpty())
        {
            return default(T);
        }

        // NOTE: the final_cast is needed so the user can modify his own data.
        T ret = top.getNode();
        HeapNode<T> newtop, i, j;

        if (top.getChild() == null) // Only one element in heap
        {
            top = null;
            size--;
            return ret;
        }

        // First of two passes: combine pairs of roots together, from
        // left to right.
        i = top.getChild();

        // Note, end condition is too complicated for more generalized loops
        while (true)
        {  // i is the first, j is the second

            if ((j = i.getNext()) == null)// If i is the odd man out (no partner j)
            {
                break;
            }

            if (i == merge(i, j))// i came out on top 
            {

                if (i.getNext() == null)// i is now the last in the loop
                {
                    break;
                }

                i = i.getNext();
            }
            else  // j came out on top
            {
                if (j.getNext() == null)// j is the last in the loop 
                {
                    i = j;  // As a set up for the next pass
                    break;
                }

                i = j.getNext();
            }
        }

        // Iterate the other way, combining each new tree with the rightmost
        newtop = i;
        j = i.getPrev();

        while (j != null)
        {
            newtop = merge(newtop, j);
            j = newtop.getPrev();
        }

        top = newtop;

        size--;

        return ret;
    }
   
    /**
    * The user must call this function to signal that a key has been decreased.
    * The function will then remove the node and subnodes from the heap, then
    * merge them back in. The user gets the heap node reference from insert.
    *
    * @param paramsdeschere
    */
    public void decreaseKey(HeapNode<T> node)
    {
        if (node == top || node == null || top == null)
        {
            return;
        }

        node.extract();

        if (merge(top, node) == node)// Node has become new top
        {
            top = node;
        }
    }

    /**
    * desc here
    *
    * @return returndeschere
    */
    public bool isEmpty()
    {
        return top == null;
    }

    /**
    * desc here
    *
    * @return returndeschere
    */
    public int getSize() { return size; }
};