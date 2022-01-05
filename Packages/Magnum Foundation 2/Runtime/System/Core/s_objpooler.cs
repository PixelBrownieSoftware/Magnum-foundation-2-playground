using MagnumFoundation2.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace MagnumFoundation2.System.Core
{
    public class s_objpooler : s_singleton<s_objpooler>
    {
        public Dictionary<string, Queue<s_object>> objectPoolList = new Dictionary<string, Queue<s_object>>();
        public List<GameObject> objPoolDatabase = new List<GameObject>();
        public List<o_character> allcharacters = new List<o_character>();
        public GameObject objectPoolerObj;

        public T AddCharacter<T>(o_character ch) where T : o_character
        {
            allcharacters.Add(ch);
            return ch.GetComponent<T>();
        }
        public T AddCharacter<T>(o_character ch, Vector2 pos) where T : o_character
        {
            allcharacters.Add(ch);
            return ch.GetComponent<T>();
        }
        public void AddCharacter(o_character ch)
        {
            allcharacters.Add(ch);
        }
        public void AddCharacter(o_character ch, Vector2 pos)
        {
            ch.transform.position = pos;
            allcharacters.Add(ch);
        }

        public void SetList()
        {
            if (objectPoolList == null)
            {
                for (int i = 0; i < objPoolDatabase.Count; i++)
                {
                    Queue<s_object> objque = new Queue<s_object>();
                    GameObject obj = objPoolDatabase[i];
                    s_object newobj = Instantiate(obj).GetComponent<s_object>();
                    if (newobj == null)
                    {
                        print("Fault at " + obj.name);
                        continue;
                    }
                    newobj.transform.SetParent(objectPoolerObj.transform);
                    objque.Enqueue(newobj);
                    newobj.gameObject.SetActive(false);
                    objectPoolList.Add(obj.name, objque);
                }
            }
        }
        public T SpawnObject<T>(string objstr, Vector3 pos, Quaternion quant) where T : s_object
        {
            T ob = null;
            if (!objectPoolList.ContainsKey(objstr))
            {
                Queue<s_object> objque = new Queue<s_object>();
                GameObject obj = objPoolDatabase.Find(x => x.name == objstr);
                s_object newobj = Instantiate(obj).GetComponent<s_object>();
                if (objectPoolerObj != null)
                    newobj.transform.SetParent(objectPoolerObj.transform);
                else
                    print("No object pooler set");
                objque.Enqueue(newobj);
                newobj.gameObject.SetActive(false);
                objectPoolList.Add(obj.name, objque);
            }
            GameObject pd = objPoolDatabase.Find(x => x.name == objstr);

            if (objectPoolList[objstr].Count < 1)
            {
                ob = Instantiate(pd, pos, quant).GetComponent<T>();
                ob.gameObject.SetActive(true);
                ob.ID = objstr;
                ob.transform.SetParent(objectPoolerObj.transform);
                return ob;
            }

            ob = objectPoolList[objstr].Dequeue().GetComponent<T>();
            ob.gameObject.transform.localRotation = quant;
            ob.transform.position = pos;
            ob.gameObject.SetActive(true);
            ob.transform.SetParent(objectPoolerObj.transform);
            ob.ID = objstr;
            return ob;
        }
        public T SpawnObject<T>(string objstr, Vector3 pos) where T : s_object
        {
            T ob = null;
            if (!objectPoolList.ContainsKey(objstr))
            {
                Queue<s_object> objque = new Queue<s_object>();
                GameObject obj = objPoolDatabase.Find(x => x.name == objstr);
                s_object newobj = Instantiate(obj).GetComponent<s_object>();
                if (objectPoolerObj != null)
                    newobj.transform.SetParent(objectPoolerObj.transform);
                else
                    print("No object pooler set");
                objque.Enqueue(newobj);
                newobj.gameObject.SetActive(false);
                objectPoolList.Add(obj.name, objque);
            }
            GameObject pd = objPoolDatabase.Find(x => x.name == objstr);

            if (objectPoolList[objstr].Count < 1)
            {
                ob = Instantiate(pd, pos, Quaternion.identity).GetComponent<T>();
                ob.gameObject.SetActive(true);
                ob.ID = objstr;
                ob.transform.SetParent(objectPoolerObj.transform);
                return ob;
            }

            ob = objectPoolList[objstr].Dequeue().GetComponent<T>();
            ob.gameObject.transform.localRotation = Quaternion.identity;
            ob.transform.position = pos;
            ob.gameObject.SetActive(true);
            ob.transform.SetParent(objectPoolerObj.transform);
            ob.ID = objstr;
            return ob;
        }

        public void DespawnObject(s_object obj)
        {
            obj.transform.parent = null;
            obj.gameObject.SetActive(false);
            if (objectPoolerObj != null)
                obj.transform.SetParent(objectPoolerObj.transform);
            if (objectPoolList.ContainsKey(obj.ID))
                objectPoolList[obj.ID].Enqueue(obj);
            else
                print("No object pooler set");
        }
    }
}
