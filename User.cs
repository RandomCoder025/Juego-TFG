using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    public String name;
    public String password;
    public int score;

    public User(string name, string password, int score)
    {
        this.name = name;
        this.password = password;
        this.score = score;
    }
}