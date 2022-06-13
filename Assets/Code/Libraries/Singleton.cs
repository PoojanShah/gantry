using UnityEngine;
//This will not prevent a non singleton constructor such as `T myT = new T();`.  To prevent that, add `protected T () {}` to your singleton class.
public class Singleton<T>:MonoBehaviour where T:MonoBehaviour{//This is made as MonoBehaviour because we need Coroutines.
	private static T _instance;
	private static object _lock = new object();
//	private static bool dontCreate=false;
	public static bool instancedYet{get{return _instance!=null;}}
	public static T instance{
		get{
			if(applicationIsQuitting){
				Debug.LogWarning("[Singleton] Instance '"+typeof(T)+"' already destroyed on application quit."+" Won't create again - returning null.");
				return null;
			}
			lock(_lock){
				if(_instance==null/*&&!dontCreate*/){
					_instance=(T)FindObjectOfType(typeof(T));
					if(FindObjectsOfType(typeof(T)).Length>1){
						Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton! There are "+FindObjectsOfType(typeof(T))+" instances of type \""+typeof(T)+"\". Reopening the scene might fix it.");
						return _instance;
					}
					if(_instance==null){
//						Debug.LogError("[Singleton] An instance of "+typeof(T)+" is needed in the scene, but not present.");
//						return null;
						GameObject singleton=new GameObject();
						_instance=singleton.AddComponent<T>();
						singleton.name="(singleton) "+typeof(T).ToString();
						DontDestroyOnLoad(singleton);
						Debug.LogWarning("[Singleton] An instance of "+typeof(T)+" is needed in the scene, so '"+singleton+"' was created with DontDestroyOnLoad.");
					}//else Debug.Log("[Singleton] Using instance already created: "+_instance.gameObject.name);
				}
				return _instance;

			}
		}
	}
	private static bool applicationIsQuitting=false;
	//When Unity quits, it destroys objects in a random order. In principle, a Singleton is only destroyed when application quits.
	//If any script calls Instance after it have been destroyed, it will create a buggy ghost object that will stay on the Editor scene, even after stopping playing the Application.
	//This was made to be sure we're not creating that buggy ghost object.
//	void Awake(){
//		Debug.Log("Awake() called for type "+typeof(T)+" on object \""+gameObject.name+"\".");
//	}
//	void Start(){
//		Debug.Log("Start() called for type "+typeof(T)+" on object \""+gameObject.name+"\".");
//	}
	public void OnApplicationQuit(){
//		Debug.Log("OnApplicationQuit() called for type "+typeof(T)+" on object \""+gameObject.name+"\". Setting applicationIsQuitting to true.");
		applicationIsQuitting=true;
	}
//	public void OnDestroy(){
//		Debug.Log("OnDestroy() called for type "+typeof(T)+" on object \""+gameObject.name+"\".");
////		applicationIsQuitting=true;
//	}
}