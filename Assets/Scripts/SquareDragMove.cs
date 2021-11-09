using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareDragMove : MonoBehaviour
{
    public GameObject end_prefab, start_prefab, obstacle_prefab, player_prefab, path_prefab;
    public GameObject left_wall, right_wall, top_wall, bottom_wall;
    private GameObject end_object, start_object, player_object;

    private GameObject[] obstacle_object;

    public PhysicsMaterial2D player_physics;
    private Rigidbody2D player_rb;
    private Collider2D player_collider, end_collider, start_collider;

    private Camera camera;

    private bool in_game = false;

    private Touch touch;

    private static bool player_collision = false;
    private static string collision_name;

    private int frames_since_last_touch = 0;

    public Text win_streak_text;
    private int win_streak = 0;

    public Text timer_text;
    private float time_since_last_start;
    private float initial_timer_start_point = 10.0f;
    private float current_timer_start_point = 10.0f;
    private float difficulty = 0.1f;

    private float math_val = -2.0f;
    private float speed_ratio = 30.0f;
    private float max_speed = 80.0f;

    private float circle_width = 0.3f;
    private float timer_ratio = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        MakeStart(new Vector2(0.5f, 0.9f));
        //scale text boxes
        Vector2 screen_scale = new Vector2(Screen.width, Screen.height);
        win_streak_text.fontSize = (int)screen_scale.y / 10;
        timer_text.fontSize = (int)screen_scale.y / 13;

        left_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(0.0f, 0.0f)));
        bottom_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(0.0f, 0.0f)));
        right_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(1.0f, 1.0f)));
        top_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(1.0f, 1.0f)));
    
    }

    public static void PlayerCollision(string name)
    {
        Debug.Log("yoink");
        player_collision = true;
        collision_name = name;
    }
    // Update is called once per frame
    void Update()
    {
        left_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(0.0f, 0.0f)));
        bottom_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(0.0f, 0.0f)));
        right_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(1.0f, 1.0f)));
        top_wall.GetComponent<Rigidbody2D>().position = (VectorRatioToWorldLocation(new Vector2(1.0f, 1.0f)));
        int touch_count = Input.touchCount;
        if (touch_count == 0)
        {
            frames_since_last_touch++;
        }
        else
        {
            frames_since_last_touch = -1;
        }

      

        if (in_game)
        {
            UpdateTimer();
            if(current_timer_start_point < time_since_last_start)
            {
                Lose();
            }
            if (player_collision)
            {
                if (collision_name == "start")
                {
                    //ignore
                }
                else if (collision_name == "end")
                {
                    Win();
                }
                else if (collision_name == "obstacle")
                {
                    Lose();
                }
                else
                {

                }
            }
            if (touch_count == 0)
            {
                player_physics.bounciness = 1.0f;
                if (frames_since_last_touch > 1)
                {
                    //Lose();
                }
            }
            else if(touch_count == 1 && in_game)
            {
                //move to tap location
                player_physics.bounciness = 0.0f;
                Vector2 target_location = VectorPixelsToWorldLocation(Input.GetTouch(0).position);
                Vector2 current_location = player_rb.position;
                Vector2 dir = (target_location - current_location).normalized;
                float mag = (target_location - current_location).magnitude;
                float value = speed_ratio * (1.0f - (float)Math.Exp(math_val * (double)mag));
                if (value > max_speed) value = max_speed;
                player_rb.velocity = new Vector2(dir.x * value, dir.y * value);
            }
            if (touch_count > 1)
            {
                Lose();
            }
        }
        else
        {
            if(touch_count == 0)
            {
                //wait
            }
            else if (touch_count == 1)
            {
                Vector2 touch_pos = VectorPixelsToWorldLocation(Input.GetTouch(0).position);
                if(start_collider == Physics2D.OverlapPoint(touch_pos))
                {
                    StartGame();
                }
            }
        }
    }
    private void ResetTimer()
    {
        time_since_last_start = 0.0f;
        current_timer_start_point = initial_timer_start_point * TimerFactor();
        //timer_text.text = (current_timer_start_point - 
        //    time_since_last_start).ToString();
        timer_text.text = "";
    }
    private void UpdateTimer()
    {
        time_since_last_start += Time.fixedDeltaTime;
        //timer_text.text = (current_timer_start_point -
        //    time_since_last_start).ToString();
        timer_text.text = "";
        timer_ratio = time_since_last_start / current_timer_start_point;
        camera.backgroundColor = new Color(timer_ratio, 0.1f, 0.1f, 1.0f);
    }
    private float TimerFactor()
    {
        return (float)Math.Pow((1.0f + difficulty), -(double)win_streak);
    }

    private void Win()
    {
        Debug.Log("win");
        Destroy(player_object);
        win_streak++;
        EndGame();
        UpdateTimer();
    }
    private void Lose()
    {
        if (!in_game) return;
        Debug.Log("lose");
        win_streak = 0;
        player_rb.velocity = new Vector2(0.0f, 0.0f);
        EndGame();
    }

    private void EndGame()
    {
        ResetTimer();
        win_streak_text.text = win_streak.ToString();
        in_game = false;
        player_collision = false;
        Destroy(end_object);
        RemoveObstacles();
        MakeStart(new Vector2(0.5f, 0.9f));
    }
    private void StartGame()
    {
        Debug.Log("start game");
        GenerateObstacles();
        Destroy(start_object);
        MakeEnd(new Vector2(0.5f, 0.1f));
        MakePlayer(new Vector2(0.5f, 0.9f));
        in_game = true;
    }

    private void GenerateObstacles()
    {
        int num_objs = 30;
        obstacle_object = new GameObject[num_objs];

        //Define a path
        float current = 0.9f, end = 0.1f, horizontal = UnityEngine.Random.Range(0.0f,1.0f);
        int last_dir = -1;
        float step = 0.05f;
        List<Vector2> cant_go_list = new List<Vector2>();
        int index = 0;
        while (current > end)
        {
            float rand = UnityEngine.Random.Range(0.0f, 1.0f);
            Vector2 vec1 = new Vector2();
            Vector2 vec2 = new Vector2();

            if(rand <= 0.2f)
            {
                vec1 = new Vector2(horizontal, current - step);
                vec2 = new Vector2(horizontal, current - 2*step);
            }
            if(rand > 0.2f && rand <= 0.6f)
            {
                if (horizontal <= 2*step) continue;
                vec1 = new Vector2(horizontal - step, current);
                vec2 = new Vector2(horizontal - 2 * step, current);
            }
            if(rand > 0.6f)
            {
                if (horizontal >= 1.0f - 2*step) continue;
                vec1 = new Vector2(horizontal + step, current);
                vec2 = new Vector2(horizontal + 2 * step, current);
            }

            if (!cant_go_list.Contains(vec2))
            {
                cant_go_list.Add(vec1);
                cant_go_list.Add(vec2);
                horizontal = vec2.x;
                current = vec2.y;
            }
            index++;
        }
        //foreach(Vector2 i in cant_go_list)
        //{
        //    Instantiate(path_prefab, VectorRatioToWorldLocation(i), Quaternion.identity);
        //}
        //return;

        Vector2[] vec_list = new Vector2[num_objs];
        for (int i = 0; i < num_objs; i++)
        {
            Vector2 obstacle_pos = new Vector2(0f,0f);
            bool cant_go_on = true;
            while (cant_go_on)
            {
                cant_go_on = false;
                float x = (UnityEngine.Random.Range(0.05f, 0.95f));
                float y = (UnityEngine.Random.Range(0.2f, 0.8f));
                obstacle_pos = new Vector2(x, y);
                //compare with path
                foreach (Vector2 obj in cant_go_list)
                {
                    if (Vector2.Distance(new Vector2(0.5f*obj.x, obj.y), new Vector2(0.5f*obstacle_pos.x, obstacle_pos.y)) < 0.09f) cant_go_on = true;
                }
                //compare with other obstacles
                for (int j = 0; j < i; j++)
                {
                    if (Vector2.Distance(vec_list[j], obstacle_pos) < 0.03f)
                        cant_go_on = true;
                }
            }
            vec_list[i] = obstacle_pos;
            obstacle_pos = VectorRatioToWorldLocation(obstacle_pos);
            obstacle_object[i] = Instantiate(obstacle_prefab, obstacle_pos, Quaternion.identity);
            obstacle_object[i].name = "obstacle";
        }
    }
    private void RemoveObstacles()
    {
        foreach(GameObject i in obstacle_object)
        {
            Destroy(i);
        }
    }

    private void MakeStart(Vector2 start_pos)
    {
        Debug.Log("called makestart");
        start_pos = VectorRatioToWorldLocation(start_pos);
        start_object = Instantiate(start_prefab, start_pos, Quaternion.identity);
        start_collider = start_object.GetComponent<Collider2D>();
        start_object.name = "start";
    }
    private void MakeEnd(Vector2 end_pos)
    {
        end_pos = VectorRatioToWorldLocation(end_pos);
        end_object = Instantiate(end_prefab, end_pos, Quaternion.identity);
        end_collider = end_object.GetComponent<Collider2D>();
        end_object.name = "end";
    }
    private void MakePlayer(Vector2 player_pos)
    {
        Destroy(player_object);
        player_pos = VectorRatioToWorldLocation(player_pos);
        player_object = Instantiate(player_prefab, player_pos, Quaternion.identity);
        player_rb = player_object.GetComponent<Rigidbody2D>();
        player_collider = player_object.GetComponent<Collider2D>();
        player_object.name = "player";
    }
    private Vector2 VectorRatioToWorldLocation(Vector2 vec_ratio)
    {
        vec_ratio.x *= Screen.width;
        vec_ratio.y *= Screen.height;
        Vector3 pos = camera.ScreenToWorldPoint(new Vector3(vec_ratio.x, vec_ratio.y, camera.nearClipPlane));
        return new Vector2(pos.x, pos.y);
    }
    private Vector2 VectorPixelsToWorldLocation(Vector2 vec_ratio)
    {
        Vector3 pos = camera.ScreenToWorldPoint(new Vector3(vec_ratio.x, vec_ratio.y, camera.nearClipPlane));
        return new Vector2(pos.x, pos.y);
    }
}
