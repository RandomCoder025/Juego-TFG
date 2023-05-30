using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : IComparable<User>
{
    public string name;
    public string password;
    public int score;

    public User(string name, string password, int score)
    {
        this.name = name;
        this.password = password;
        this.score = score;
    }

    public int CompareTo(User other)
    {
        return score.CompareTo(other.score);
    }
}