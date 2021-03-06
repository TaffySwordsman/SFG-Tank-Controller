using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class PhaseManager : MonoBehaviour
{
    public event Action<int> PhaseChange = delegate { };
    int phase = 0, curHealth, maxHealth;
    bool headRaised = false;
    [SerializeField] BossController _boss = null;
    [SerializeField] MeshRenderer CRT;
    [SerializeField] Color phase0Color, phase1Color, phase2Color, deathColor;
    private Material screenMat;
    [SerializeField] [Range(0, 1)] float phase1Health, phase2Health;
    [SerializeField] GameObject[] faces = new GameObject[3];
    DamageableObject _health = null;
    [SerializeField] DamageableObject _leftHealth, _rightHealth;

    private void Awake()
    {
        screenMat = CRT.materials[1];
        _health = GetComponent<DamageableObject>();
        _health.OnTakeDamage += Damaged;
        _leftHealth.OnTakeDamage += Damaged;
        _rightHealth.OnTakeDamage += Damaged;
        PhaseChange += ChangePhase;
        _boss.ReadyMissiles.Invoke();
    }

    void Damaged()
    {
        curHealth = _health.GetHealth();
        maxHealth = _health.GetMaxHealth();
        if (curHealth <= maxHealth * 0.9f  && !headRaised)
            _boss.HideHead.Invoke();

        if ((_leftHealth.GetHealth() <= _leftHealth.GetMaxHealth() * 0.5f || _rightHealth.GetHealth() <= _rightHealth.GetMaxHealth() * 0.5f) && !headRaised)
        {
            _boss.ShowHead.Invoke();
            headRaised = true;

            _leftHealth.invincible = true;
            _rightHealth.invincible = true;
        }

        if(curHealth <= 0){
            _leftHealth.invincible = false;
            _rightHealth.invincible = false;
            _leftHealth.Kill();
            _rightHealth.Kill();
        }

        if (curHealth <= maxHealth * phase2Health)
        {
            screenMat.color = Color.Lerp(phase2Color, deathColor, Mathf.InverseLerp(maxHealth * phase2Health, 0, curHealth));
            if (phase == 1)
                PhaseChange.Invoke(2);
        }
        else if (curHealth <= maxHealth * phase1Health)
        {
            screenMat.color = Color.Lerp(phase1Color, phase2Color, Mathf.InverseLerp(maxHealth * phase1Health, maxHealth * phase2Health, curHealth));
            if (phase == 0)
            {
                PhaseChange.Invoke(1);
                _boss.UnreadyMissiles.Invoke();
            }
        }
        else
        {
            screenMat.color = Color.Lerp(phase0Color, phase1Color, Mathf.InverseLerp(maxHealth, maxHealth * phase1Health, curHealth));
        }
    }

    void ChangePhase(int newPhase)
    {
        phase = newPhase;
        _boss.phase = newPhase;

        for (int i = 0; i < 3; i++)
        {
            if (i == newPhase)
                faces[i].SetActive(true);
            else { faces[i].SetActive(false); }
        }
    }
}
