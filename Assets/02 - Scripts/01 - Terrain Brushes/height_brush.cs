using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class height_brush : TerrainBrush {

    public float height = 10;
    //public int radius_modif = 20;
    //public string shape = "rectangle";
    
    public enum shape_list {rectangle,circle};
    public shape_list list1;
    int x_int,z_int;
    double angle;
    public override void draw(int x, int z) {
        
        if(list1==shape_list.rectangle){
            for (int zi = -radius; zi <= radius; zi++) {
                for (int xi = -radius; xi <= radius; xi++) {
                    terrain.set(x + xi, z + zi, terrain.get(x+xi,z+zi)+height);
                    
                }
            }
        }

        if(list1==shape_list.circle){
            for (int zi = -radius; zi <= radius; zi++) {
                for (int xi = -radius; xi <= radius; xi++) {
                    if((Math.Pow(xi,2) + Math.Pow(zi,2))<Math.Pow(radius,2)){
                        terrain.set(x + xi, z + zi, terrain.get(x+xi,z+zi)+height);
                    }
                }
            }
        }
        
        
    }
}
